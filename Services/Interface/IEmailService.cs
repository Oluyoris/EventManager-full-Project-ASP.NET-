using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}