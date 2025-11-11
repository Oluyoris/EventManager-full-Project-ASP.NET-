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
    public class PlannerService : IPlannerService
    {
        private readonly EventManagerDbContext _context;

        public PlannerService(EventManagerDbContext context)
        {
            _context = context;
        }

        public async Task<User> UpdateProfileAsync(int plannerId, UserUpdateDto updateDto)
        {
            var planner = await _context.Users.FirstOrDefaultAsync(u => u.Id == plannerId && u.Role == UserRole.Planner);
            if (planner == null)
                throw new ArgumentException("Planner not found.");

            planner.FullName = updateDto.FullName ?? planner.FullName;
            planner.PhoneNumber = updateDto.PhoneNumber ?? planner.PhoneNumber;
            planner.CountryCode = updateDto.CountryCode ?? planner.CountryCode;
            planner.Country = updateDto.Country ?? planner.Country;
            planner.CompanyName = updateDto.CompanyName ?? planner.CompanyName;
            planner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return planner;
        }

        public async Task UpdatePasswordAsync(int plannerId, PasswordUpdateDto passwordDto)
        {
            var planner = await _context.Users.FirstOrDefaultAsync(u => u.Id == plannerId && u.Role == UserRole.Planner);
            if (planner == null)
                throw new ArgumentException("Planner not found.");

            if (!BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, planner.PasswordHash))
                throw new ArgumentException("Current password is incorrect.");

            planner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            planner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<User> GetProfileAsync(int plannerId)
        {
            var planner = await _context.Users.FirstOrDefaultAsync(u => u.Id == plannerId && u.Role == UserRole.Planner);
            if (planner == null)
                throw new ArgumentException("Planner not found.");

            return planner;
        }
    }
}