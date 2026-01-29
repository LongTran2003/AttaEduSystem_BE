using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities
{
    public class UserUsage : BaseEntity<string, string, string>
    {
        [Key]
        public Guid UserUsageId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        // Khoảng thời gian áp dụng quota (theo tháng)
        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        public int TokensUsed { get; set; }
        public int ScansUsed { get; set; }
        public int GeneratedExamsUsed { get; set; }
    }
}
