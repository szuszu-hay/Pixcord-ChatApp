namespace ChatAPI.Models
{
    public class MessageReaction
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Message Message { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public string Emoji { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}