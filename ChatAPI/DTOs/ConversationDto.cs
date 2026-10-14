namespace ChatAPI.DTOs
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? IconUrl { get; set; }
        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsOnline { get; set; }
        public string MessageTo { get; set; } = string.Empty;
        public bool CanMessage { get; set; } = true;
    }
}