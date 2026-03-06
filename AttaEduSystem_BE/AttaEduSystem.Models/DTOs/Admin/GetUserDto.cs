namespace AttaEduSystem.Models.DTOs.Admin
{
    public class GetUserDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime? LockoutEnd { get; set; }

        // Additional info
        public string? StudentCode { get; set; } // Nếu là Student
        public string? TeacherCode { get; set; } // Nếu là Teacher
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
