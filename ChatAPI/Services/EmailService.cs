using System.Net;
using System.Net.Mail;
using ChatAPI.Configuration;
using ChatAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ChatAPI.Services
{
    public class EmailService: IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService (IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        //Send Email Confirmation
        public async Task SendEmailConfirmationAsync (string email, string displayName, string username, string confirmationUrl)
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential (_emailSettings.SenderEmail, _emailSettings.Password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress (_emailSettings.SenderEmail, "ChatApp"),
                Subject = "Verify Your ChatApp Email Address",
                IsBodyHtml = true
            };
            
            mail.To.Add(email);
            mail.Body = $"""
            <!DOCTYPE html>
            <html>
                <body style="font-family: Arial, sans-serif;">
                    <h2>Welcome to ChatApp, {WebUtility.HtmlEncode(displayName)}!</h2>
                    <p>Thanks for creating your ChatApp account.</p>
                    <p>Before you can log in, please verify your email address.</p>

                    <p>
                        <strong>Your username:</strong>
                        {WebUtility.HtmlEncode(username)}
                    </p>

                    <p>
                        <a href="{WebUtility.HtmlEncode(confirmationUrl)}"
                           style="display:inline-block; padding:12px 20px; background:#5865F2;
                           color:white; text-decoration:none; border-radius:6px;">
                            Verify my email
                        </a>
                    </p>

                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{WebUtility.HtmlEncode(confirmationUrl)}</p>

                    <p>If you didn't create a ChatApp account, you can safely ignore this email.</p>
                    <p>Thanks,<br />The ChatApp Team</p>
                </body>
            </html>
            """;

            await client.SendMailAsync(mail);
        }
    }
}