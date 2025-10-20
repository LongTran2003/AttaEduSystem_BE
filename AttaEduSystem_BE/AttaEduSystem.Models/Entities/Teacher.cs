using AttaEduSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class Teacher
    {
        [Key]
        public Guid TeacherId { get; set; }
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")] public virtual ApplicationUser ApplicationUser { get; set; } = null!;
        [StringLength(8)] public string TeacherCode { get; set; } = null!;
        public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    }
}
