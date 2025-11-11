using EventManager.Api.Dtos;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<PaymentMethodDto> UpdatePaymentMethodAsync(int adminId, PaymentMethodUpdateDto paymentMethodDto);
        Task<SiteSettingsDto> UpdateSiteSettingsAsync(int adminId, SiteSettingsUpdateDto settingsDto);
        Task<PaymentMethodDto> GetPaymentMethodAsync();
        Task<SiteSettingsDto?> GetSiteSettingsAsync(); // Changed to allow null return
    }
}