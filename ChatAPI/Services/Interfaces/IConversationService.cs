using ChatAPI.DTOs;

namespace ChatAPI.Services.Interfaces
{
    public interface IConversationService
    {
        Task<ConversationDto?> CreateDirectConversationAsync(string currentUserId, string otherUserId);
        Task<ConversationDto?> CreateGroupConversationAsync(string currentUserId, CreateGroupConversationDto dto);
        Task<List<ConversationDto>> GetConversationsAsync(string userId);
        Task<ConversationDetailsDto?> GetConversationDetailsAsync(int conversationId, string userId);
        Task<ConversationMemberDto?> AddMemberAsync(int conversationId, string actingUserId, string targetUserId);
        Task<bool> RemoveMemberAsync(int conversationId, string actingUserId, string targetUserId);
        Task<bool> LeaveConversationAsync(int conversationId, string userId);
        Task<MessageDto?> SendMessageAsync(int conversationId, string userId, string content);
        Task<ConversationDto?> UpdateGroupDetailsAsync (int conversationId, string userId, string? name, IFormFile? iconFile, IWebHostEnvironment env);
        Task<List<MessageDto>?> GetMessagesAsync(int conversationId, string userId);
        Task<ConversationDto?> GetConversationByIdAsync(int conversationId, string userId);
        Task<MessageDto?> EditMessageAsync(int conversationId, int messageId, string userId, string content);
        Task<bool> DeleteMessageAsync(int conversationId, int messageId, string userId);
        Task<List<ReactionSummaryDto>?> ToggleReactionAsync(int conversationId, int messageId, string userId, string emoji);
        Task<MessageDto?> SendMessageWithAttachment(int conversationId, string userId, string? content, IFormFile file, IWebHostEnvironment env);
    }
}