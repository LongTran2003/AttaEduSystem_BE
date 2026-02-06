using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttaEduSystem.Models.Entities;

public class ExamFolder : BaseEntity<string, string, string>
{
    [Key]
    public Guid FolderId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!; // VD: "Toán Hình 12", "Ôn thi HK1"

    // Màu sắc hoặc icon folder (Optional - làm màu cho App đẹp)
    public string? ColorCode { get; set; } = "#FFC107"; // Mặc định màu vàng folder

    // Folder của ai?
    public string UserId { get; set; } = null!;
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    // Danh sách đề thi trong folder này
    public virtual ICollection<ExamPaper> ExamPapers { get; set; } = new List<ExamPaper>();
}