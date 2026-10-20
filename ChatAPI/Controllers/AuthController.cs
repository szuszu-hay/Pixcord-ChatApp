using ChatAPI.DTOs;
using ChatAPI.Services;
using ChatAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _authService.LoginAsync(dto);

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = await _authService.RegisterAsync(dto);
            return Created("", user);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var confirmed = await _authService.ConfirmEmailAsync(userId, token);
            if (!confirmed)
            {
                return Redirect(
                    "http://localhost:5173/email-confirmation-failed"
                );
            }

             return Redirect(
                "http://localhost:5173/email-confirmed"
            );
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            try
            {
                var result = await _authService.GoogleLoginAsync(dto.IdToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}