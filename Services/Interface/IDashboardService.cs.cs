using EventManager.Api.Dtos;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(string userId);
    }
}