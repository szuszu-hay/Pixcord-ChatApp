using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class UpdateUsernameDto
    {
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
    }
}