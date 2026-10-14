namespace ChatAPI.Models
{
    public class ConversationMember
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public bool IsMuted { get; set; }
        public DateTime? MutedUntil { get; set; }
        public int? LastReadMessageId { get; set; }
    }
}