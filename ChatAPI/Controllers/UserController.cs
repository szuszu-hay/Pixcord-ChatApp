using ChatAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatAPI.DTOs;
using ChatAPI.Services.Interfaces;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUsernameService _usernameService;

        public UserController(IUserService userService, IUsernameService usernameService)
        {
            _userService = userService;
            _usernameService = usernameService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var users = await _userService.SearchUsersAsync(query, currentUserId);

            return Ok(users);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPut("me/username")]
        public async Task<IActionResult> UpdateMyUsername([FromBody] UpdateUsernameDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var profile = await _usernameService.UpdateUsernameAsync(userId, dto);
                if (profile == null) return NotFound();
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("me/bio")]
        public async Task<IActionResult> UpdateMyBio([FromBody] UpdateBioDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var profile = await _userService.UpdateBioAsync(userId, dto);
                if (profile == null) return NotFound();
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("me/avatar")]
        public async Task<IActionResult> UpdateMyAvatar([FromForm] IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var profile = await _userService.UpdateAvatarAsync(userId, file);
                if (profile == null) return NotFound();
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}