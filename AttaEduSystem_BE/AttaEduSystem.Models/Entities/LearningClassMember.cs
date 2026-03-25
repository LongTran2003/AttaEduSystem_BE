using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities;

public class LearningClassMember : BaseEntity<string, string, string>
{
    [Key]
    public Guid LearningClassMemberId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid LearningClassId { get; set; }

    [ForeignKey("LearningClassId")]
    public virtual LearningClass LearningClass { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = null!;

    public DateTime JoinedAt { get; set; }
}
