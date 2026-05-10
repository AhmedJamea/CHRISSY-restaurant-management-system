using Models.DTOs.Auth;
using Models.Entites;

namespace Business.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginExternalAsync(string email);
    }
}
