using EventManager.Api.Dtos;
using EventManager.Api.Models;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IPlannerService
    {
        Task<User> UpdateProfileAsync(int plannerId, UserUpdateDto updateDto);
        Task UpdatePasswordAsync(int plannerId, PasswordUpdateDto passwordDto);
        Task<User> GetProfileAsync(int plannerId);
    }
}