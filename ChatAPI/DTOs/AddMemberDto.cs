using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class AddMemberDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}