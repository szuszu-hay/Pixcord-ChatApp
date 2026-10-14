using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class SendMessageDto
    {
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}