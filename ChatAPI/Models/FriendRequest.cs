namespace ChatAPI.Models
{
    public class FriendRequest
    {
        public int Id { get; set; } 
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = null!;
        public string ReceiverId { get; set; } = string.Empty;
        public ApplicationUser Receiver { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
        public FriendRequestStatus Status { get; set; }
    }
}