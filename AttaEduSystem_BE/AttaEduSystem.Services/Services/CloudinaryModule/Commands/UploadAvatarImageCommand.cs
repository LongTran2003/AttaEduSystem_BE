using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;

namespace AttaEduSystem.Services.Services.CloudinaryModule.Commands
{
    public class UploadAvatarImageCommand : ICommand
    {
        private ICloudinaryService _cloudinaryService;
        private readonly IFormFile _file;
        private readonly string _folderPath;
        public UploadAvatarImageCommand(ICloudinaryService cloudinaryService, IFormFile file, string folderPath)
        {
            _cloudinaryService = cloudinaryService;
            _file = file;
            _folderPath = folderPath;
        }
        public async Task ExecuteAsync()
        {
            await _cloudinaryService.UploadImageAsync(
                _file,
                _folderPath,
                new Transformation().Named(
                    StaticCloudinarySettings.StudentAvatarTransformation));
        }
    }
}
