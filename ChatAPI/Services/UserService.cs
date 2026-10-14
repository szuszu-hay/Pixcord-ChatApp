using ChatAPI.Data;
using ChatAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using ChatAPI.Models;
using Microsoft.AspNetCore.Identity;
using ChatAPI.Helpers;
using ChatAPI.Services.Interfaces;

namespace ChatAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Search user
        public async Task<List<UserSearchResultDto>> SearchUsersAsync(string query, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<UserSearchResultDto>();
            }

            query = query.Trim();

            var friendIds = _context.FriendRequests
                .Where(f =>
                    f.Status == FriendRequestStatus.Accepted &&
                    (f.SenderId == currentUserId || f.ReceiverId == currentUserId))
                .Select(f => f.SenderId == currentUserId ? f.ReceiverId : f.SenderId);

            return await _context.Users
                .Where(u => u.Id != currentUserId && u.EmailConfirmed && !friendIds.Contains(u.Id) && (
                    u.UserName!.Contains(query) || u.DisplayName.Contains(query)
                ))
                .Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    Username = u.UserName!,
                    DisplayName = u.DisplayName,
                    ProfilePictureUrl = u.ProfilePictureUrl ?? ""
                })
                .Take(20)
                .ToListAsync();
        }


        // User Profile
        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt
            };
        }

        // Update Bio
        public async Task<UserProfileDto?> UpdateBioAsync(string userId, UpdateBioDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return new UserProfileDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt
            };
        }

        // Update avatar
        public async Task<UserProfileDto?> UpdateAvatarAsync(string userId, IFormFile file, IWebHostEnvironment env)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            user.ProfilePictureUrl = await FileStorage.SaveAsync(file, "avatars", FileStorage.ImageTypes, env);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return new UserProfileDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt
            };
        }

    }
}