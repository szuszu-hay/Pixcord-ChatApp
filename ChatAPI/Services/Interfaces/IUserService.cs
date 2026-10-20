using ChatAPI.DTOs;
using ChatAPI.Models;

namespace ChatAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserSearchResultDto>> SearchUsersAsync(string query, string currentUserId);
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<UserProfileDto?> UpdateBioAsync(string userId, UpdateBioDto dto);
        Task<UserProfileDto?> UpdateAvatarAsync(string userId, IFormFile file);
    }
}