using Models.DTOs.Auth;

namespace Business.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task RegisterAsync(RegisterRequestDto request);
    }
}
