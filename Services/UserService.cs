using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using BCrypt.Net;

namespace EventManager.Api.Services
{
    public class UserService : IUserService
    {
        private readonly EventManagerDbContext _context;

        public UserService(EventManagerDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterAsync(UserRegisterDto registerDto)
        {
            if (string.IsNullOrEmpty(registerDto.Username) || string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
                throw new ArgumentException("Username, email, and password are required.");

            if (registerDto.Password != registerDto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
                throw new ArgumentException("Username or email already exists.");

            var user = new User
            {
                FullName = registerDto.FullName,
                Username = registerDto.Username,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                CountryCode = registerDto.CountryCode,
                Country = registerDto.Country,
                CompanyName = registerDto.CompanyName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = UserRole.Planner,
                IsBlocked = false,
                IsEmailVerified = false,
                Balance = 0.0m,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> LoginAsync(UserLoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.UsernameOrEmail) || string.IsNullOrEmpty(loginDto.Password))
                throw new ArgumentException("Username/email and password are required.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.UsernameOrEmail || u.Email == loginDto.UsernameOrEmail)
                ?? throw new ArgumentException("Invalid username or email.");

            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new ArgumentException("Invalid password.");

            return user;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new ArgumentException("User not found.");
            return user;
        }
    }
}