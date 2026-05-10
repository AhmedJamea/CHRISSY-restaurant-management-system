using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Auth;
using Models.Entites;
using System.Security.Claims;

namespace Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // This makes the URL: api/auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(
            IAuthService authService,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            _authService = authService;
            _signInManager = signInManager;
            _userManager = userManager;
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

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            // Set the callback URL to the response endpoint
            string redirectUrl = Url.Action("GoogleResponse", "Auth");

            // Configure the external login properties for Google
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            // This sends the "Challenge" to the browser
            return Challenge(properties, "Google");
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            // 1. Extract info from Google's handshake
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return BadRequest(new { message = "Error loading Google information." });

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            try
            {
                // 2. Ask the Service to handle everything
                var response = await _authService.LoginExternalAsync(email);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Reuses your generic "Invalid email or password" message
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An internal error occurred." });
            }
        }
    }
}