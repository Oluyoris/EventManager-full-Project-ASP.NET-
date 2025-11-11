using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace EventManager.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly EventManagerDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(EventManagerDbContext context, ILogger<AdminService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<User> UpdateProfileAsync(int adminId, UserUpdateDto updateDto)
        {
            _logger.LogInformation("Updating profile for admin ID: {AdminId}", adminId);
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId && u.Role == UserRole.Admin);
            if (admin == null)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            admin.FullName = updateDto.FullName ?? admin.FullName;
            admin.PhoneNumber = updateDto.PhoneNumber ?? admin.PhoneNumber;
            admin.CountryCode = updateDto.CountryCode ?? admin.CountryCode;
            admin.Country = updateDto.Country ?? admin.Country;
            admin.CompanyName = updateDto.CompanyName ?? admin.CompanyName;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Profile updated successfully for admin ID: {AdminId}", adminId);
            return admin;
        }

        public async Task UpdatePasswordAsync(int adminId, PasswordUpdateDto passwordDto)
        {
            _logger.LogInformation("Updating password for admin ID: {AdminId}", adminId);
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId && u.Role == UserRole.Admin);
            if (admin == null)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, admin.PasswordHash))
            {
                _logger.LogError("Current password incorrect for admin ID: {AdminId}", adminId);
                throw new ArgumentException("Current password is incorrect.");
            }

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Password updated successfully for admin ID: {AdminId}", adminId);
        }

        public async Task<User> GetProfileAsync(int adminId)
        {
            _logger.LogInformation("Fetching profile for admin ID: {AdminId}", adminId);
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId && u.Role == UserRole.Admin);
            if (admin == null)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            _logger.LogInformation("Profile fetched successfully for admin ID: {AdminId}", adminId);
            return admin;
        }

        public async Task<List<User>> GetAllPlannersAsync()
        {
            _logger.LogInformation("Fetching all planners.");
            var planners = await _context.Users
                .Where(u => u.Role == UserRole.Planner)
                .ToListAsync();
            _logger.LogInformation("Fetched {Count} planners.", planners.Count);
            return planners;
        }

        public async Task<User> GetPlannerByIdAsync(int plannerId)
        {
            _logger.LogInformation("Fetching planner by ID: {PlannerId}", plannerId);
            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
            {
                _logger.LogError("Planner not found for ID: {PlannerId}", plannerId);
                throw new ArgumentException("Planner not found.");
            }

            _logger.LogInformation("Planner fetched successfully for ID: {PlannerId}", plannerId);
            return planner;
        }

        public async Task<User> BlockPlannerAsync(int adminId, int plannerId)
        {
            _logger.LogInformation("Blocking planner ID: {PlannerId} by admin ID: {AdminId}", plannerId, adminId);
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
            {
                _logger.LogError("Planner not found for ID: {PlannerId}", plannerId);
                throw new ArgumentException("Planner not found.");
            }

            planner.IsBlocked = true;
            planner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Planner ID: {PlannerId} blocked successfully.", plannerId);
            return planner;
        }

        public async Task<User> DeletePlannerAsync(int adminId, int plannerId)
        {
            _logger.LogInformation("Deleting planner ID: {PlannerId} by admin ID: {AdminId}", plannerId, adminId);
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
            {
                _logger.LogError("Planner not found for ID: {PlannerId}", plannerId);
                throw new ArgumentException("Planner not found.");
            }

            _context.Users.Remove(planner);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Planner ID: {PlannerId} deleted successfully.", plannerId);
            return planner;
        }

        public async Task<Event> CreateEventForPlannerAsync(int adminId, int plannerId, EventCreateDto eventDto)
        {
            _logger.LogInformation("Creating event for planner ID: {PlannerId} by admin ID: {AdminId}", plannerId, adminId);
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
            {
                _logger.LogError("Planner not found for ID: {PlannerId}", plannerId);
                throw new ArgumentException("Planner not found.");
            }

            var @event = new Event
            {
                EventName = eventDto.EventName,
                Location = eventDto.Location,
                Date = eventDto.Date,
                Time = eventDto.Time,
                Description = eventDto.Description,
                NumberOfGuests = eventDto.NumberOfGuests,
                UserId = plannerId,
                Status = EventStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Event created successfully with ID: {EventId} for planner ID: {PlannerId}", @event.Id, plannerId);
            return @event;
        }

        public async Task<User> UpdatePlannerBalanceAsync(int adminId, int plannerId, decimal amount)
        {
            _logger.LogInformation("Updating balance for planner ID: {PlannerId} by admin ID: {AdminId}, Amount: {Amount}", plannerId, adminId, amount);
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
            {
                _logger.LogError("Planner not found for ID: {PlannerId}", plannerId);
                throw new ArgumentException("Planner not found.");
            }

            planner.Balance += amount;
            if (planner.Balance < 0) planner.Balance = 0;

            planner.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Balance updated successfully for planner ID: {PlannerId}, New Balance: {Balance}", plannerId, planner.Balance);
            return planner;
        }

        public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync()
        {
            _logger.LogInformation("Fetching dashboard analytics.");
            var totalEvents = await _context.Events.CountAsync();
            var totalUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Planner);
            var totalIncome = await _context.Transactions
                .Where(t => t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.Amount);
            var totalTransactions = await _context.Transactions.CountAsync();
            var totalUpcomingEvents = await _context.Events
                .CountAsync(e => e.Date > DateTime.UtcNow && e.Status != EventStatus.Completed);
            var totalCompletedEvents = await _context.Events
                .CountAsync(e => e.Date <= DateTime.UtcNow || e.Status == EventStatus.Completed);

            var analytics = new DashboardAnalyticsDto
            {
                TotalEvents = totalEvents,
                TotalUsers = totalUsers,
                TotalIncome = totalIncome,
                TotalTransactions = totalTransactions,
                TotalUpcomingEvents = totalUpcomingEvents,
                TotalCompletedEvents = totalCompletedEvents
            };

            _logger.LogInformation("Dashboard analytics fetched successfully: TotalEvents={TotalEvents}, TotalUsers={TotalUsers}", totalEvents, totalUsers);
            return analytics;
        }

        public async Task<PaymentMethod> GetPaymentSettingsAsync()
        {
            _logger.LogInformation("Fetching payment settings.");
            var settings = await _context.PaymentMethods.FirstOrDefaultAsync();
            if (settings == null)
            {
                _logger.LogError("Payment settings not found.");
                throw new ArgumentException("Payment settings not found.");
            }

            _logger.LogInformation("Payment settings fetched successfully.");
            return settings;
        }

        public async Task<PaymentMethod> UpdatePaymentSettingsAsync(int adminId, PaymentMethodUpdateDto updateDto)
        {
            _logger.LogInformation("Updating payment settings by admin ID: {AdminId}", adminId);
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            var settings = await _context.PaymentMethods.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new PaymentMethod
                {
                    IsManualActive = updateDto.IsManualActive,
                    BankName = updateDto.BankName,
                    AccountName = updateDto.AccountName,
                    AccountNumber = updateDto.AccountNumber,
                    IsPaystackActive = updateDto.IsPaystackActive,
                    PaystackPublicKey = updateDto.PaystackPublicKey,
                    PaystackSecretKey = updateDto.PaystackSecretKey,
                    IsFlutterwaveActive = updateDto.IsFlutterwaveActive,
                    FlutterwavePublicKey = updateDto.FlutterwavePublicKey,
                    FlutterwaveSecretKey = updateDto.FlutterwaveSecretKey,
                    IsPaypalActive = updateDto.IsPaypalActive,
                    PaypalClientId = updateDto.PaypalClientId,
                    PaypalClientSecret = updateDto.PaypalClientSecret,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PaymentMethods.Add(settings);
                _logger.LogInformation("Created new payment settings.");
            }
            else
            {
                settings.IsManualActive = updateDto.IsManualActive;
                settings.BankName = updateDto.BankName ?? settings.BankName;
                settings.AccountName = updateDto.AccountName ?? settings.AccountName;
                settings.AccountNumber = updateDto.AccountNumber ?? settings.AccountNumber;
                settings.IsPaystackActive = updateDto.IsPaystackActive;
                settings.PaystackPublicKey = updateDto.PaystackPublicKey ?? settings.PaystackPublicKey;
                settings.PaystackSecretKey = updateDto.PaystackSecretKey ?? settings.PaystackSecretKey;
                settings.IsFlutterwaveActive = updateDto.IsFlutterwaveActive;
                settings.FlutterwavePublicKey = updateDto.FlutterwavePublicKey ?? settings.FlutterwavePublicKey;
                settings.FlutterwaveSecretKey = updateDto.FlutterwaveSecretKey ?? settings.FlutterwaveSecretKey;
                settings.IsPaypalActive = updateDto.IsPaypalActive;
                settings.PaypalClientId = updateDto.PaypalClientId ?? settings.PaypalClientId;
                settings.PaypalClientSecret = updateDto.PaypalClientSecret ?? settings.PaypalClientSecret;
                _logger.LogInformation("Updated existing payment settings.");
            }

            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Payment settings updated successfully by admin ID: {AdminId}", adminId);
            return settings;
        }

        public async Task<int> GetPendingTransactionsCountAsync()
        {
            _logger.LogInformation("Fetching pending transactions count.");
            var count = await _context.Transactions
                .CountAsync(t => t.Status == TransactionStatus.Pending);
            _logger.LogInformation("Pending transactions count: {Count}", count);
            return count;
        }
    }
}