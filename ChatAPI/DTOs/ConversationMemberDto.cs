namespace ChatAPI.DTOs
{
    public class ConversationMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty; 
        public string Username { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public bool IsOnline { get; set; }
    }
}