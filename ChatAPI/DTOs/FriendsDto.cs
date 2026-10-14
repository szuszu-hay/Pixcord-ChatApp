using ChatAPI.Models;

namespace ChatAPI.DTOs
{
    public class FriendsDto
    {
        public string FriendId { get; set; } = string.Empty;
        public string FriendUsername { get; set; } = string.Empty;
        public string FriendDisplayName { get; set; } = string.Empty;
        public string? FriendProfilePictureUrl { get; set; }
        public string? FriendBio { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? FriendSince { get; set; }
    }
}