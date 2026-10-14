using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class CreateGroupConversationDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        [Required]
        [MinLength(2, ErrorMessage="A group conversation must have at least 2 users.")]
        public List<string> MemberIds { get; set; } = new List<string>();
    }
}