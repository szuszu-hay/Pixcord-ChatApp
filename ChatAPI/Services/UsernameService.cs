using ChatAPI.Models;
using ChatAPI.DTOs;
using Microsoft.AspNetCore.Identity;
using ChatAPI.Services.Interfaces;

namespace ChatAPI.Services
{
    public class UsernameService : IUsernameService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UsernameService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        //Generate Username
        public async Task<string> GenerateUsernameAsync(string displayName)
        {
            var baseUsername = new string(
                displayName.ToLowerInvariant()
                    .Where(char.IsLetterOrDigit)
                    .ToArray()
            );

            if (string.IsNullOrWhiteSpace(baseUsername))
            {
                baseUsername = "user";
            }

            var username = $"{baseUsername}";

            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser == null)
            {
                return username;
            }

            var random = new Random();
            do
            {
                var random_num = random.Next(1000, 9999);
                username = $"@{baseUsername}{random_num}";

                existingUser = await _userManager.FindByNameAsync(username);
            } while (existingUser != null);

            return username;
        }

        public async Task<UserProfileDto?> UpdateUsernameAsync(string userId, UpdateUsernameDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            user.UserName = string.IsNullOrWhiteSpace(dto.Username) ? null : dto.Username.Trim();

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
    }
}