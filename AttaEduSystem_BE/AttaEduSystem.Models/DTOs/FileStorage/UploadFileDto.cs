using Microsoft.AspNetCore.Http;

namespace AttaEduSystem.Models.DTOs.FileStorage
{
    public class UploadFileDto
    {
        public IFormFile? File { get; set; }
    }
}
