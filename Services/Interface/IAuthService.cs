using EventManager.Api.Models; // Changed from EventManager.Api.Data to EventManager.Api.Models
using EventManager.Api.Dtos;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserRegisterDto registerDto);
        Task<string> LoginAsync(UserLoginDto loginDto);
        Task UpdatePasswordAsync(string userId, PasswordUpdateDto passwordDto);
    }
}