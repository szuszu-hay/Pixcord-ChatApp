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

        public static string GetExtension(string contentType) => ExtensionsByContentType[contentType];

        public static void Validate(IFormFile file, string[] allowedContentTypes)
        {
            if (file.Length == 0)
                throw new InvalidOperationException("File is empty.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("File exceeds the 10 MB limit.");

            if (!allowedContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException("File type not allowed.");
        }
    }
}