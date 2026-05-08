using Models.DTOs.Auth;

namespace Business.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user (Admin or Cashier) and returns a JWT token if successful.
        /// </summary>
        /// <param name="request">The login credentials (username and password).</param>
        /// <returns>An AuthResponseDto containing the token and user details, or null if authentication fails.</returns>
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    }
}
