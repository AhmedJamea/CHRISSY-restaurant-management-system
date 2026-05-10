using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
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
            try
            {
                var response = await _authService.LoginAsync(loginRequest);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                // This catches your "Invalid email or password" message.
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // This catches the "Account configuration error" (missing branch).
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // For database crashes or unexpected bugs.
                return StatusCode(500, new { message = "An internal error occurred." });
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> register([FromBody] RegisterRequestDto RegisterRequest)
        {
            try
            {
                await _authService.RegisterAsync(RegisterRequest);
                return Ok(new { message = "User created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
            }
        }

    }
}