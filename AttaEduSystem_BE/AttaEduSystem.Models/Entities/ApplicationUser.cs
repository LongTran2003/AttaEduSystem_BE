using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string FullName { get; set; } = null!;

        public DateTime BirthDate { get; set; }

        [StringLength(100)]
        public string? Address { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        [StringLength(6)]
        public string? OtpCode { get; set; }

        public DateTime? OtpExpiry { get; set; }

        public string Status { get; set; } = "Active"; // Default status is Active

    }
}
