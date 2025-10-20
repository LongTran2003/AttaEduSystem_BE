using AttaEduSystem.Utilities.ValidationAttribute;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.DTOs.Authentication
{
    public class SignUpStudentDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Password]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [ConfirmPassword("Password")]
        [NotMapped]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public string Gender { get; set; } = null!;

        [Required]
        public DateTime BirthDate { get; set; }
    }
}
