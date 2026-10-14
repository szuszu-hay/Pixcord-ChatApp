namespace ChatAPI.DTOs
{
    public class ConversationListDto
    {
        public int ConversationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}