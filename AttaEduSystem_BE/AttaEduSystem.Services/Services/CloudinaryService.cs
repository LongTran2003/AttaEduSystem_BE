using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.Models.DTOs.Cloudinary;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AttaEduSystem.Services.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        public IConfiguration Configuration { get; }
        private CloudinarySettingsDto settings;
        private Cloudinary cloudinary;
        private readonly ApplicationDBContext _context;

        public CloudinaryService(IConfiguration configuration, ApplicationDBContext context)
        {
            Configuration = configuration;
            settings = Configuration.GetSection(StaticCloudinarySettings.CloudinarySettingsSection).Get<CloudinarySettingsDto>();
            Account account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            cloudinary = new Cloudinary(account);
            this._context = context;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderPath, Transformation? transformation = null)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(
                   file.FileName.Replace(" ", ","),
                   file.OpenReadStream()),
                PublicId = Guid.NewGuid().ToString(),
                Folder = folderPath,
                UseFilename = true,
                UniqueFilename = false,
                Transformation = transformation
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }

            throw new Exception($"Failed to upload image to Cloudinary. Error: {uploadResult.Error.Message}");
        }

        public async Task<string> GetImageUrlAsync(string publicId)
        {
            var getResourceParams = new GetResourceParams(publicId);

            try
            {
                var resource = await cloudinary.GetResourceAsync(getResourceParams);
                return resource.SecureUrl.ToString();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to retrieve image from Cloudinary. Error: {e.Message}");
            }
        }

        public async Task<string> UploadVideoAsync(IFormFile file, string folderPath)
        {
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(
                   file.FileName.Replace(" ", ","),
                   file.OpenReadStream()),
                PublicId = Guid.NewGuid().ToString(),
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }

            throw new Exception($"Failed to upload video to Cloudinary. Error: {uploadResult.Error.Message}");
        }

        /// <summary>
        /// Upload raw file (PDF, etc.) from byte array
        /// </summary>
        public async Task<string> UploadRawFileAsync(byte[] fileBytes, string fileName, string folderPath)
        {
            await using var stream = new MemoryStream(fileBytes);

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = folderPath,
                PublicId = Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.UtcNow.Ticks
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            // Return URL as-is (without fl_attachment)
            return uploadResult.SecureUrl.ToString();
        }

    }
}
