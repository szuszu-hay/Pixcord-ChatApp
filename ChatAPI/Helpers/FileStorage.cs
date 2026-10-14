namespace ChatAPI.Helpers
{
    public static class FileStorage
    {
        public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        private static readonly Dictionary<string, string> ExtensionsByContentType = new()
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/gif"] = ".gif",
            ["image/webp"] = ".webp",
            ["application/pdf"] = ".pdf",
            ["text/plain"] = ".txt"
        };

        public static readonly string[] ImageTypes =
            { "image/jpeg", "image/png", "image/gif", "image/webp" };

        public static readonly string[] AttachmentTypes =
            { "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf", "text/plain" };

        public static async Task<string> SaveAsync(
            IFormFile file, string folderName, string[] allowedContentTypes, IWebHostEnvironment env)
        {
            if (file.Length == 0)
                throw new InvalidOperationException("File is empty.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("File exceeds the 10 MB limit.");

            if (!allowedContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException("File type not allowed.");

            var folderPath = Path.Combine(env.WebRootPath, folderName);
            Directory.CreateDirectory(folderPath);

            // extension comes from the validated content type, never from the client's filename
            var safeFileName = $"{Guid.NewGuid()}{ExtensionsByContentType[file.ContentType]}";
            var filePath = Path.Combine(folderPath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folderName}/{safeFileName}";
        }
    }
}