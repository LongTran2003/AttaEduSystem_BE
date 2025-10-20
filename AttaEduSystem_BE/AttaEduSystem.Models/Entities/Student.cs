using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class Student
    {
        [Key]
        public Guid StudentId { get; set; }

        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")] public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
