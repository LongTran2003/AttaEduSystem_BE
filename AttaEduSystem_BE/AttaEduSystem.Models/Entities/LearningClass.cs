using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities;

public class LearningClass : BaseEntity<string, string, string>
{
    [Key]
    public Guid LearningClassId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(30)]
    public string? SchoolYear { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Schedule { get; set; }

    [Required]
    public string OwnerUserId { get; set; } = null!;

    [ForeignKey("OwnerUserId")]
    public virtual ApplicationUser OwnerUser { get; set; } = null!;

    public virtual ICollection<LearningClassMember> Members { get; set; } = new List<LearningClassMember>();

    [StringLength(20)]
    public string EnrollKey { get; set; } = string.Empty;
}
