namespace ChatAPI.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public ConversationType Type { get; set; }
        public string? Name { get; set; }
        public string? IconUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    }
}