using System.Net;
using ChatAPI.DTOs;
using ChatAPI.Exceptions;
using ChatAPI.Models;
using Microsoft.AspNetCore.Identity;
using ChatAPI.Services.Interfaces;
using Google.Apis.Auth;

namespace ChatAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService _tokenService;
        private readonly UsernameService _usernameService;
        private readonly IEmailService _emailService;     
        private readonly IConfiguration _configuration;                         

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
                            TokenService tokenService, UsernameService usernameService, IEmailService emailService, 
                            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _usernameService = usernameService;
            _emailService = emailService;
            _configuration = configuration;
        }

        //Login
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var matchingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (matchingUser == null)
            {
                throw new InvalidCredentialsException();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(matchingUser, dto.Password, true);
            if (result.IsLockedOut)
            {
                throw new AccountLockedOutException();
            }

            if (!matchingUser.EmailConfirmed)
            {
                throw new EmailNotConfirmedException();
            }
        
            if (!result.Succeeded)
            {
                throw new InvalidCredentialsException();
            }

            var token = _tokenService.GenerateToken(matchingUser);
            return new LoginResponseDto
            {
                Token = token,
                UserId = matchingUser.Id,
                Email = matchingUser.Email ?? string.Empty,
                DisplayName = matchingUser.DisplayName
            };
        }

        //Register Account
        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException();
            }

            var username = await _usernameService.GenerateUsernameAsync(dto.DisplayName);

            var user = new ApplicationUser
            {
                UserName = username,
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var confirmationUrl = $"http://localhost:5156/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailConfirmationAsync(
                user.Email,
                user.DisplayName,
                user.UserName!,
                confirmationUrl
            );

            return new RegisterResponseDto
            {
                UserId = user.Id,
                DisplayName = user.DisplayName,
                Username = user.UserName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }

        //Email Confirmation
        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            if (user.EmailConfirmed) return true;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        //Google login
        public async Task<ApplicationUser> GetOrCreateGoogleUserAsync(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;
            Console.WriteLine($"Backend Google ClientId: '{_configuration["Google:ClientId"]}'");
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration["Google:ClientId"]! }
                    });
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine($"Google token validation failed: {ex.Message}");
                throw new InvalidOperationException("Invalid Google sign-in.");
            }

            if (!payload.EmailVerified)
                throw new InvalidOperationException("Your Google email isn't verified.");

            var user = await _userManager.FindByLoginAsync("Google", payload.Subject);
            if (user != null) return user;

            user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                var displayName = payload.Name ?? payload.Email.Split('@')[0];
                if (displayName.Length > 30) displayName = displayName[..30];

                user = new ApplicationUser
                {
                    UserName = await _usernameService.GenerateUsernameAsync(displayName),
                    Email = payload.Email,
                    DisplayName = displayName,
                    EmailConfirmed = true, 
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _userManager.CreateAsync(user); 
                if (!created.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(", ", created.Errors.Select(e => e.Description)));
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                if (await _userManager.HasPasswordAsync(user))
                    await _userManager.RemovePasswordAsync(user);
            }

            var linked = await _userManager.AddLoginAsync(user,
                new UserLoginInfo("Google", payload.Subject, "Google"));
            if (!linked.Succeeded)
                throw new InvalidOperationException("Couldn't link your Google account.");

            return user;
        }

        //Login
        public async Task<LoginResponseDto?> GoogleLoginAsync(string idToken)
        {
            var user = await GetOrCreateGoogleUserAsync(idToken);
            
            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new AccountLockedOutException();
            }

            return new LoginResponseDto
            {
                Token = _tokenService.GenerateToken(user),
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName
            };
        }
    }
}