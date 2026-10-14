using ChatAPI.Models;

namespace ChatAPI.DTOs
{
    public class FriendRequestResponseDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public string? SenderProfilePictureUrl { get; set; }

        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverUsername { get; set; } = string.Empty;
        public string ReceiverDisplayName { get; set; } = string.Empty;
        public string? ReceiverProfilePictureUrl { get; set; }

        public FriendRequestStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}