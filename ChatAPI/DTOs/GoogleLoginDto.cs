using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}