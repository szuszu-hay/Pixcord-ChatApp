namespace ChatAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync (string email, string displayName, string username, string confirmationUrl);
    }
}