using EventManager.Api.Dtos;
using EventManager.Api.Models;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterAsync(UserRegisterDto registerDto);
        Task<User> LoginAsync(UserLoginDto loginDto);
        Task<User> GetUserByIdAsync(int userId);
    }
}