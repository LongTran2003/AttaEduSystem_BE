using Microsoft.AspNetCore.Http;

namespace AttaEduSystem.Services.IServices
{
    public interface IOcrService
    {
        Task<string> ExtractText(IFormFile imageFile);
    }
}
