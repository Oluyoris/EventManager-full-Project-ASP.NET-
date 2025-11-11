using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly EventManagerDbContext _context;

        public DashboardService(EventManagerDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId) || parsedUserId <= 0)
            {
                throw new ArgumentException("Invalid user ID format. User ID must be a positive integer.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == parsedUserId);
            if (user == null)
            {
                Console.WriteLine($"User not found for ID: {userId}");
                throw new ArgumentException("User not found.");
            }

            var events = await _context.Events
                .Where(e => e.UserId == parsedUserId)
                .ToListAsync();

            var totalEvents = events.Count;
            var completedEvents = events.Count(e => e.Status == EventStatus.Completed);
            var upcomingEvents = events.Count(e => e.Date > DateTime.UtcNow && e.Status != EventStatus.Completed);

            // Include Draft events in upcoming if date is in the future
            upcomingEvents += events.Count(e => e.Date > DateTime.UtcNow && e.Status == EventStatus.Draft);

            var recentEvents = await _context.Events
                .Where(e => e.UserId == parsedUserId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .Select(e => new EventDto
                {
                    Id = e.Id.ToString(),
                    EventName = e.EventName,
                    Location = e.Location,
                    Date = e.Date.ToString("yyyy-MM-dd"),
                    Time = e.Time,
                    Description = e.Description,
                    NumberOfGuests = e.NumberOfGuests,
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync();

            decimal walletBalance = user.Balance;

            var dashboard = new DashboardDto
            {
                PlannerName = user.FullName ?? user.Username,
                TotalEvents = totalEvents,
                CompletedEvents = completedEvents,
                UpcomingEvents = upcomingEvents,
                WalletBalance = walletBalance,
                RecentEvents = recentEvents
            };

            Console.WriteLine($"Dashboard data for user {userId}: {Newtonsoft.Json.JsonConvert.SerializeObject(dashboard)}");
            return dashboard;
        }
    }
}