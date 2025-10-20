using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.FileStorage;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Services.Services.CloudinaryModule.Invoker;
using AttaEduSystem.Utilities.Constants;
using CloudinaryDotNet;
using System.Security.Claims;


namespace AttaEduSystem.Services.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly CloudinaryServiceControl _cloudinaryServiceControl;
        private readonly ICloudinaryService _cloudinaryService;

        public FileStorageService(
            CloudinaryServiceControl cloudinaryServiceControl,
            ICloudinaryService cloudinaryService)
        {
            _cloudinaryServiceControl = cloudinaryServiceControl;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ResponseDto> UploadAvatarImage(UploadFileDto uploadFileDto, ClaimsPrincipal user)
        {
            if (user.FindFirstValue(ClaimTypes.NameIdentifier) is null)
            {
                return ErrorResponse.Build(
                    message: StaticOperationStatus.User.UserNotFound,
                    statusCode: StaticOperationStatus.StatusCode.NotFound);
            }

            if (uploadFileDto.File?.Length is 0)
            {
                return ErrorResponse.Build(
                    message: StaticOperationStatus.File.FileEmpty,
                    statusCode: StaticOperationStatus.StatusCode.BadRequest);
            }

            var folderPath = $"{StaticCloudinaryFolders.StudentAvatars}/{user.FindFirstValue("FullName")}";
            try // Upload image to Cloudinary
            {
                var uploadedImageUrl = await _cloudinaryService.UploadImageAsync(
                    uploadFileDto.File,
                    folderPath,
                    new Transformation().Named(StaticCloudinarySettings.StudentAvatarTransformation));

                return SuccessResponse.Build(
                    message: StaticOperationStatus.File.FileUploaded,
                    statusCode: StaticOperationStatus.StatusCode.Ok,
                    result: uploadedImageUrl);
            }
            catch (Exception e)
            {
                return ErrorResponse.Build(
                    message: e.Message,
                    statusCode: StaticOperationStatus.StatusCode.InternalServerError);
            }
        }
    }
}
