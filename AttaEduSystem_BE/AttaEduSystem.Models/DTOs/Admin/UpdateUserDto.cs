using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Admin
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(50)]
        public string FullName { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(100)]
        public string? Address { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } // Active, Inactive, Suspended
    }
}
