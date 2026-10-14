namespace ChatAPI.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Message { get; set; }
        public int? ConversationId { get; set; }
        public string? RelatedUserId { get; set; }
        public string? RelatedUserDisplayName { get; set; }
        public string? RelatedUserProfilePictureUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}