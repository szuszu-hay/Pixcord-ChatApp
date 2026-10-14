namespace ChatAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? Message { get; set; }
        public int? ConversationId { get; set; }
        public string? RelatedUserId { get; set; }
        public ApplicationUser? RelatedUser { get; set; }
        public string? RelatedMessageId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    } 
}