namespace ChatAPI.DTOs
{
    public class ConversationDetailsDto
    {
        public int ConversationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? IconUrl { get; set; }
        // Group
        public List<ConversationMemberDto> Members { get; set; } = new();
        // Direct
        public string? Username { get; set; }
        public string? OtherUserId { get; set; }
        public string? OtherUserBio { get; set; }
        public DateTime? OtherUserJoinedAt { get; set; }
        public DateTime? FriendSince { get; set; }
    }
}