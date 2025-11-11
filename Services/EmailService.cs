using EventManager.Api.Data;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EventManager.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly EventManagerDbContext _context;

        public EmailService(EventManagerDbContext context)
        {
            _context = context;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            if (settings == null)
                throw new InvalidOperationException("Site settings not configured.");

            if (settings.EmailProvider == "SMTP")
            {
                if (string.IsNullOrEmpty(settings.SmtpUsername))
                    throw new InvalidOperationException("SMTP username not configured.");

                using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                    EnableSsl = settings.SmtpUseSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.SmtpUsername),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            else if (settings.EmailProvider == "SendGrid")
            {
                var apiKey = settings.SendGridApiKey;
                if (string.IsNullOrEmpty(apiKey))
                    throw new InvalidOperationException("SendGrid API key not configured.");

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("no-reply@eventmanager.com", "EventManager");
                var toAddress = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, body, body);
                await client.SendEmailAsync(msg);
            }
            else
            {
                throw new InvalidOperationException("Invalid email provider configured.");
            }
        }
    }
}