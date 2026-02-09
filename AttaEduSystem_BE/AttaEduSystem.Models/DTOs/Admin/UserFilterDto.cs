namespace AttaEduSystem.Models.DTOs.Admin
{
    public class UserFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; } // Search by email, name
        public string? Role { get; set; } // Filter by role: Student, Teacher, Admin
        public string? Status { get; set; } // Filter by status: Active, Inactive
        public string? SortBy { get; set; } // email, fullName, createdAt
        public string? SortOrder { get; set; } = "asc"; // asc, desc
    }
}
