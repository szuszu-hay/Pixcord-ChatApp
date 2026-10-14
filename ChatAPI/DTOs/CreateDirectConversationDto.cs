using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class CreateDirectConversationDto
    {
        [Required]
        public string FriendId { get; set; } = string.Empty;
    }
}