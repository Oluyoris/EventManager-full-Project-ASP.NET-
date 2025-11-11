using EventManager.Api.Dtos;
using EventManager.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IAdminService
    {
        Task<User> UpdateProfileAsync(int adminId, UserUpdateDto updateDto);
        Task UpdatePasswordAsync(int adminId, PasswordUpdateDto passwordDto);
        Task<User> GetProfileAsync(int adminId);
        Task<List<User>> GetAllPlannersAsync();
        Task<User> GetPlannerByIdAsync(int plannerId);
        Task<User> BlockPlannerAsync(int adminId, int plannerId);
        Task<User> DeletePlannerAsync(int adminId, int plannerId);
        Task<Event> CreateEventForPlannerAsync(int adminId, int plannerId, EventCreateDto eventDto);
        Task<User> UpdatePlannerBalanceAsync(int adminId, int plannerId, decimal amount);
        Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync();
        Task<PaymentMethod> GetPaymentSettingsAsync();
        Task<PaymentMethod> UpdatePaymentSettingsAsync(int adminId, PaymentMethodUpdateDto updateDto);
        Task<int> GetPendingTransactionsCountAsync();
    }
}