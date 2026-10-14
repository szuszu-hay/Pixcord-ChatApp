namespace ChatAPI.Models {
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
        public string SenderId { get; set; } = string.Empty; 
        public ApplicationUser Sender { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
        public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
        public MessageType Type { get; set; } = MessageType.Text;
    }
}