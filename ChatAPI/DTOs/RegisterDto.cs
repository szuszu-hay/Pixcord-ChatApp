using System.ComponentModel.DataAnnotations;
namespace ChatAPI.DTOs
{
    public class RegisterDto
    {
        [Required] [StringLength(30, MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty; 

        [Required] [EmailAddress]
        public string Email { get; set; } = string.Empty; 

        [Required] [MinLength(12)]
        public string Password { get; set; } = string.Empty; 

        [Required] [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}