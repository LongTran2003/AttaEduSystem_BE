using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;

namespace AttaEduSystem.Services.IServices
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Async method to upload an image to Cloudinary.
        /// </summary>
        /// <param name="file">The image file to be uploaded</param>
        /// <param name="folderPath">The path to the specific folder that stores the image</param>
        /// <param name="transformation">The transformation to be applied to the image (null by default)</param>
        /// <returns>An URL string of the uploaded resource</returns>
        /// <exception cref="Exception">If the upload operation failed</exception>
        Task<string> UploadImageAsync(IFormFile file, string folderPath, Transformation? transformation = null);
        /// <summary>
        /// Async method to get an image from Cloudinary based on the public ID.
        /// </summary>
        /// <param name="publicId">The image's public ID, combination of (folder path + file name)</param>
        /// <returns>An URL string of the retrieved image</returns>
        /// <exception cref="Exception">If the retrieval operation failed</exception>
        Task<string> GetImageUrlAsync(string publicId);
        Task<string> UploadVideoAsync(IFormFile file, string folderPath);
    }
}
