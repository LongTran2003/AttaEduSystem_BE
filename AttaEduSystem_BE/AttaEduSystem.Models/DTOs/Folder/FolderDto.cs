namespace AttaEduSystem.Models.DTOs.Folder;

public class FolderDto
{
    public Guid FolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorCode { get; set; } = "#FFC107";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public int ExamCount { get; set; } // Số lượng đề trong folder
}