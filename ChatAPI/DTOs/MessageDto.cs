namespace ChatAPI.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public string? SenderProfilePictureUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public List<ReactionSummaryDto> Reactions { get; set; } = new();
        public List<AttachmentDto> Attachments { get; set; } = new();
        public string Type { get; set; } = "Text";
    }
}