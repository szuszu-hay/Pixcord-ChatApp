using ChatAPI.DTOs;
using ChatAPI.Models;

namespace ChatAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
        Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<LoginResponseDto?> GoogleLoginAsync(string idToken);
        Task<ApplicationUser> GetOrCreateGoogleUserAsync(string idToken);
    }
}