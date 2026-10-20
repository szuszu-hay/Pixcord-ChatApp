using ChatAPI.Helpers;
using ChatAPI.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ChatAPI.Services
{
    public class CloudinaryStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"] ?? throw new InvalidOperationException("Cloudinary:CloudName is missing."),
                config["Cloudinary:ApiKey"] ?? throw new InvalidOperationException("Cloudinary:ApiKey is missing."),
                config["Cloudinary:ApiSecret"] ?? throw new InvalidOperationException("Cloudinary:ApiSecret is missing."));

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> SaveAsync(IFormFile file, string folderName, string[] allowedContentTypes)
        {
            FileStorage.Validate(file, allowedContentTypes);

            var id = Guid.NewGuid().ToString();
            var isImage = FileStorage.ImageTypes.Contains(file.ContentType);

            await using var stream = file.OpenReadStream();
            UploadResult result;

            if (isImage)
            {
                result = await _cloudinary.UploadAsync(new ImageUploadParams
                {
                    File = new FileDescription(id, stream),
                    PublicId = $"chatapp/{folderName}/{id}"
                });
            }
            else
            {
                var name = id + FileStorage.GetExtension(file.ContentType);
                result = await _cloudinary.UploadAsync(new RawUploadParams
                {
                    File = new FileDescription(name, stream),
                    PublicId = $"chatapp/{folderName}/{name}"
                });
            }

            if (result.Error != null)
                throw new InvalidOperationException($"Upload failed: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }
    }
}