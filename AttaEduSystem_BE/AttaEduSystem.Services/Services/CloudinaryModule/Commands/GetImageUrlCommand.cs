using AttaEduSystem.Services.IServices;
using System.Windows.Input;

namespace AttaEduSystem.Services.Services.CloudinaryModule.Commands
{
    public class GetImageUrlCommand : ICommand
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly string _publicId;

        public GetImageUrlCommand(ICloudinaryService cloudinaryService, string publicId)
        {
            _cloudinaryService = cloudinaryService;
            _publicId = publicId;
        }

        public async Task ExecuteAsync()
        {
            await _cloudinaryService.GetImageUrlAsync(_publicId);
        }
    }
}
