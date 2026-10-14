using System.Security.Claims;
using ChatAPI.DTOs;
using ChatAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/friend-requests")]
    [Authorize]
    public class FriendRequestController : ControllerBase
    {
        private readonly IFriendRequestService _friendRequestService;

        public FriendRequestController(IFriendRequestService friendRequestService)
        {
            _friendRequestService = friendRequestService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        public async Task<IActionResult> SendRequest(SendFriendRequestDto dto)
        {
            var request = await _friendRequestService.SendRequestAsync(UserId, dto);

            return Ok(request);
        }

        [HttpGet("incoming")]
        public async Task<IActionResult> GetIncomingRequests()
        {
            var requests = await _friendRequestService.GetIncomingRequestsAsync(UserId);

            return Ok(requests);
        }

        [HttpGet("outgoing")]
        public async Task<IActionResult> GetOutgoingRequests()
        {
            var requests = await _friendRequestService.GetOutgoingRequestsAsync(UserId);

            return Ok(requests);
        }

        [HttpPut("{requestId}/accept")]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var request = await _friendRequestService.AcceptRequestAsync(UserId, requestId);

            return Ok(request);
        }

        [HttpPut("{requestId}/reject")]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var request = await _friendRequestService.RejectRequestAsync(UserId, requestId);

            return Ok(request);
        }

        [HttpPut("{requestId}/cancel")]
        public async Task<IActionResult> CancelRequest(int requestId)
        {
            var request = await _friendRequestService.CancelRequestAsync(UserId, requestId);

            return Ok(request);
        }

        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            var friends = await _friendRequestService.GetFriendsAsync(UserId);
            return Ok(friends);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFriends([FromQuery] string query)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var friends = await _friendRequestService.SearchFriendAsync(query, currentUserId);
            return Ok(friends);
        }

        [HttpDelete("friends/{friendId}")]
        public async Task<IActionResult> Unfriend(string friendId)
        {
            var success = await _friendRequestService.UnfriendAsync(UserId, friendId);
            if (!success) return NotFound(new { message = "You're not friends with this person." });
            return NoContent();
        }
    }
}