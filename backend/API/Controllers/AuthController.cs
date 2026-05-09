using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Auth;

namespace Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // This makes the URL: api/auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // We inject the interface, not the implementation
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            // 1. Call the Business Logic Layer
            var result = await _authService.LoginAsync(loginRequest);

            // 2. If the service returns null (invalid user/pass/branch rule), return 401
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username, password, or branch assignment." });
            }

            // 3. Otherwise, return the DTO with the JWT token
            return Ok(result);
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> register([FromBody] RegisterRequestDto RegisterRequest)
        {
            var result = await _authService.RegisterAsync(RegisterRequest);

            if (result == false)
            {
                return BadRequest(new { message = "Registration failed. Please check user details or branch assignment." });
            }

            return Ok(new { message = "User created successfully." });
        }

    }
}