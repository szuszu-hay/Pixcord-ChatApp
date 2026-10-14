using System.Security.Claims;
using ChatAPI.DTOs;
using ChatAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/conversations")]
    public class ConversationController : ControllerBase
    {
        private readonly ConversationService _conversationService;

        public ConversationController(ConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] CreateDirectConversationDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) 
                return Unauthorized();
            
            var conversation = await _conversationService.CreateDirectConversationAsync(currentUserId!, dto.FriendId);
            if (conversation == null)
            {
                return BadRequest("Add user first to start a conversation.");
            }
            return Ok(conversation);
        }

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupConversationDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) 
                return Unauthorized();
            
            var conversation = await _conversationService.CreateGroupConversationAsync(currentUserId!, dto);
            if (conversation == null)
            {
                return BadRequest("Failed to create group conversation.");
            }
            return Ok(conversation);
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var conversations = await _conversationService.GetConversationsAsync(UserId);
            return Ok(conversations);
        }

        [HttpGet("{conversationId}/details")]
        public async Task<IActionResult> GetConversationDetails(int conversationId)
        {
            var details = await _conversationService.GetConversationDetailsAsync(conversationId, UserId);
            if (details == null) return Forbid();
            return Ok(details);
        }

        [HttpPost("{conversationId}/add-members")]
        public async Task<IActionResult> AddMember(int conversationId, [FromBody] AddMemberDto dto)
        {
            try
            {
                var member = await _conversationService.AddMemberAsync(conversationId, UserId, dto.UserId);
                if (member == null) return Forbid();
                return Ok(member);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{conversationId}/members/{targetUserId}")]
        public async Task<IActionResult> RemoveMember(int conversationId, string targetUserId)
        {
            var success = await _conversationService.RemoveMemberAsync(conversationId, UserId, targetUserId);
            if (!success) return Forbid();
            return NoContent();
        }

        [HttpPost("{conversationId}/leave")]
        public async Task<IActionResult> LeaveConversation(int conversationId)
        {
            var success = await _conversationService.LeaveConversationAsync(conversationId, UserId);
            if (!success) return Forbid();
            return NoContent();
        }

        [HttpPut("{conversationId}/group")]
        public async Task<IActionResult> UpdateGroupDetails(
            int conversationId, [FromForm] string? name, [FromForm] IFormFile? iconFile,
            [FromServices] IWebHostEnvironment env)
        {
            try
            {
                var conversation = await _conversationService.UpdateGroupDetailsAsync(conversationId, UserId, name, iconFile, env);
                if (conversation == null) return Forbid();
                return Ok(conversation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversation(int conversationId)
        {
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId, UserId);

            if (conversation == null) return Forbid();
            return Ok(conversation);
        }
        
        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(int conversationId, [FromBody] SendMessageDto dto)
        {
            try
            {
                var message = await _conversationService.SendMessageAsync(conversationId, UserId, dto.Content);
                if (message == null) return Forbid();
                return Ok(message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var messages = await _conversationService.GetMessagesAsync(conversationId, UserId);
            if (messages == null) return Forbid();
            return Ok(messages);
        }

        [HttpPut("{conversationId}/messages/{messageId}")]
        public async Task<IActionResult> EditMessage(int conversationId, int messageId, [FromBody] SendMessageDto dto)
        {
            var message = await _conversationService.EditMessageAsync(conversationId, messageId, UserId, dto.Content);

            if (message == null)
                return Forbid();

            return Ok(message);
        }

        [HttpDelete("{conversationId}/messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int conversationId, int messageId)
        {
            var success = await _conversationService.DeleteMessageAsync(conversationId, messageId, UserId);

            if (!success)
                return Forbid();

            return NoContent();
        }

        [HttpPost("{conversationId}/messages/{messageId}/reactions")]
        public async Task<IActionResult> ToggleReaction(int conversationId, int messageId, [FromBody] ToggleReactionDto dto)
        {
            var reactions = await _conversationService.ToggleReactionAsync(conversationId, messageId, UserId, dto.Emoji);
                                        
            if (reactions == null)
                return Forbid();

            return Ok(reactions);
        }

        [HttpPost("{conversationId}/messages/upload")]
        public async Task<IActionResult> SendMessagesWithAttachment(int conversationId, [FromForm] string? content, [FromForm] IFormFile file, [FromServices] IWebHostEnvironment env)
        {

            try
            {
                var message = await _conversationService.SendMessageWithAttachment(conversationId, UserId, content, file, env);
                if (message == null) return Forbid();

                return Ok(message); 
            } catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
    }
}