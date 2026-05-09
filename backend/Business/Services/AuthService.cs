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
            // 1. find the user by username
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return null;
            }

            // 2. Verify the Password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return null;
            }

            // 3. Get User Roles
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "None";

            // 4. ENFORCE BUSINESS RULES
            if (primaryRole == "Cashier" && user.BranchId == null)
            {
                return null;
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

        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {
            if (request.Role.Equals("Cashier", StringComparison.OrdinalIgnoreCase) && !request.BranchId.HasValue)
            {
                return false;
            }

            var user = new User
            {
                UserName = request.Username,
                Name = request.Name,
                // Admin may have no branch assignment
                BranchId = request.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? null : request.BranchId
            };

            // Create the User in the AspNetUsers table
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

                if (!roleResult.Succeeded)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private string GenerateJwtToken(User user, IList<string> roles)
        {
            // 1.Create the Claims(The data embedded inside the token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            // Add the BranchId to the token if it exists
            if (user.BranchId.HasValue)
            {
                claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
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