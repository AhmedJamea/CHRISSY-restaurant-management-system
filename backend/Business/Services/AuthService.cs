using Business.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.DTOs.Auth;
using Models.Entites;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            // 1. Find the user by username
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // 2. Verify the Password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // 3. Get User Roles
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "None";

            // 4. Check if cashier is assigned to a branch
            if (primaryRole.Equals("Cashier", StringComparison.OrdinalIgnoreCase) && user.BranchId == null)
            {
                throw new InvalidOperationException("Account configuration error: No branch assigned.");
            }

            // 5. Generate the JWT Token
            var token = GenerateJwtToken(user, roles);

            // 6. Return the formatted response
            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Role = primaryRole,
                BranchId = user.BranchId
            };
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            // Explicit Check for Existing Email
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"The email address '{request.Email}' is already registered to another staff member.");
            }

            if (request.Role.Equals("Cashier", StringComparison.OrdinalIgnoreCase) && !request.BranchId.HasValue)
            {
                throw new InvalidOperationException("A Cashier must be assigned to a specific branch.");
            }

            var user = new User
            {
                UserName = request.Email,
                Name = request.Name,
                Email = request.Email,
                // Admin may have no branch assignment
                BranchId = request.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? null : request.BranchId
            };

            // Create the User in the AspNetUsers table
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                // Concatenate all Identity errors into one message
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User creation failed: {errorMessages}");
            }

            // Create the User role in the AspNetUsersRoles table
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Role assignment failed for user {user.Email}. Please contact an admin.");
            }
        }

        private string GenerateJwtToken(User user, IList<string> roles)
        {
            // 1.Create the Claims(The data embedded inside the token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };
            // Add the BranchId to the token if it exists
            if (user.BranchId.HasValue)
            {
                claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
            }

            // Add the roles to the token
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 2. Get the Secret Key from appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Build the Token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            // 4. Serialize to a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}