using AttaEduSystem.Models.Enums;

namespace AttaEduSystem.Models.DTOs.Student
{
    public class GetStudentDto
    {
        public string StudentId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string? Gender { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public string? ImageUrl { get; set; } = null!;
        public StudentStatus Status { get; set; }

    }
}
