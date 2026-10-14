using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTOs
{
    public class UpdateBioDto
    {
        [MaxLength(200)]
        public string? Bio { get; set; }
    }
}