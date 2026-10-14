using ChatAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace ChatAPI.Data
{
    public static class UserSeeder
    {
        private record SeedUser(string Email, string Password, string DisplayName);

        private static readonly List<SeedUser> UsersToSeed = new()
        {
            new SeedUser("my@email.com", "myPassword1!", "Szu Szu"),
            new SeedUser("alice@email.com", "alicePassword1!", "Alice Johnson"),
            new SeedUser("bob@email.com", "bobPassword1!", "Bob Smith"),
            new SeedUser("carla@email.com", "carlaPassword1!", "Carla Diaz"),
            new SeedUser("tom@email.com", "tomPassword1!", "Tom"),
            new SeedUser("charles@email.com", "charlesPassword1!", "Charles"),
            new SeedUser("user@email.com", "userPassword1!", "Blossom"),
            new SeedUser("may@email.com", "mayPassword1!", "May"),
        };

        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager)
        {
            foreach (var seedUser in UsersToSeed)
            {
                var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
                if (existingUser != null)
                {
                    continue;
                }

                var user = new ApplicationUser
                {
                    UserName = seedUser.Email,
                    Email = seedUser.Email,
                    EmailConfirmed = true,
                    DisplayName = seedUser.DisplayName,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, seedUser.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(error => error.Description)
                    );

                    throw new Exception(
                        $"Failed to create seed user {seedUser.Email}: {errors}"
                    );
                }
            }
        }
    }
}