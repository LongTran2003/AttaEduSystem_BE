using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Models.DTOs.Folder;

public class CreateFolderDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    public string? ColorCode { get; set; } = "#FFC107";
}