namespace ChatAPI.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string folderName, string[] allowedContentTypes);
    }
}