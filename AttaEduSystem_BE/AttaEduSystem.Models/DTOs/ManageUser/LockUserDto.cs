using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.ManageUser
{
    public class LockUserDto
    {
        [Required]
        public string UserId { get; set; } = null!;
        public DateTimeOffset? LockoutEndDate { get; set; }
    }
}
