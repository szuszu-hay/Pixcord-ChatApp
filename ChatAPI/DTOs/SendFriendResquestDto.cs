using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class SendFriendRequestDto
    {
        [Required]
        public string ReceiverId { get; set; } = string.Empty;
    }
}