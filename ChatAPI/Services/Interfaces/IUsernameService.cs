using ChatAPI.DTOs;
using ChatAPI.Models;

namespace ChatAPI.Services.Interfaces
{
    public interface IUsernameService
    {
        Task<string> GenerateUsernameAsync(string displayName);
        Task<UserProfileDto?> UpdateUsernameAsync(string userId, UpdateUsernameDto dto);
    }
}