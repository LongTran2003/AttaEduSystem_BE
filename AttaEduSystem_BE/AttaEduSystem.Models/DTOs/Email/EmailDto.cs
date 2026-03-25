using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Email
{
    public class EmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
