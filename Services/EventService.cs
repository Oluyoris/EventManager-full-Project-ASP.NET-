using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Api.Services
{
    public class EventService : IEventService
    {
        private readonly EventManagerDbContext _context;
        private readonly IQrCodeService _qrCodeService;

        public EventService(EventManagerDbContext context, IQrCodeService qrCodeService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
        }

        public async Task<Event> CreateEventAsync(int plannerId, EventCreateDto eventDto)
        {
            var planner = await _context.Users.FindAsync(plannerId) ?? throw new ArgumentException("Planner not found.");
            if (planner.Role != UserRole.Planner)
                throw new ArgumentException("Invalid role.");

            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            if (siteSettings == null)
            {
                Console.WriteLine("SiteSettings not found in EventService.CreateEventAsync.");
                throw new ArgumentException("Site settings not found.");
            }
            Console.WriteLine($"SiteSettings found in EventService: {siteSettings.SiteName}, {siteSettings.EventPricePerGuest}");

            var eventCost = siteSettings.EventPricePerGuest * eventDto.NumberOfGuests;
            var guestDetailsCost = eventDto.AddGuestDetails ? siteSettings.GuestDetailsFee * eventDto.NumberOfGuests : 0;
            var totalCost = eventCost + guestDetailsCost;
            Console.WriteLine($"Calculated total cost: {totalCost}, Planner balance: {planner.Balance}");
            if (planner.Balance < totalCost)
                throw new ArgumentException("Insufficient balance. Please top up.");

            var @event = new Event
            {
                EventName = eventDto.EventName,
                Location = eventDto.Location,
                Date = eventDto.Date,
                Time = eventDto.Time,
                Description = eventDto.Description,
                NumberOfGuests = eventDto.NumberOfGuests,
                PlannerId = plannerId,
                Status = EventStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            planner.Balance -= totalCost;
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            // Generate guests and QR codes
            var guestList = new List<Guest>();
            var eventNamePrefix = (@event.EventName?.Length >= 4 ? @event.EventName.Substring(0, 4) : @event.EventName ?? "EVNT").ToUpper();
            if (eventDto.AddGuestDetails && eventDto.Guests != null && eventDto.Guests.Count == eventDto.NumberOfGuests)
            {
                foreach (var guestDto in eventDto.Guests)
                {
                    if (string.IsNullOrEmpty(guestDto.Name))
                        throw new ArgumentException("Guest name cannot be null or empty.");

                    var qrCodeDto = new GenerateQrCodeDto
                    {
                        EventId = @event.Id,
                        GuestName = guestDto.Name,
                        GuestEmail = guestDto.Email
                    };
                    var qrCode = await _qrCodeService.GenerateQrCodeAsync(qrCodeDto);

                    var guest = new Guest
                    {
                        EventId = @event.Id,
                        Name = guestDto.Name,
                        Email = guestDto.Email,
                        QrCode = qrCode.QrCodeValue,
                        Status = GuestStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    guestList.Add(guest);
                }
            }
            else
            {
                for (int i = 1; i <= eventDto.NumberOfGuests; i++)
                {
                    var guestName = $"{eventNamePrefix}-{(i).ToString("D4")}";
                    var qrCodeDto = new GenerateQrCodeDto
                    {
                        EventId = @event.Id,
                        GuestName = guestName,
                        GuestEmail = null
                    };
                    var qrCode = await _qrCodeService.GenerateQrCodeAsync(qrCodeDto);

                    var guest = new Guest
                    {
                        EventId = @event.Id,
                        Name = guestName,
                        Email = null,
                        QrCode = qrCode.QrCodeValue,
                        Status = GuestStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    guestList.Add(guest);
                }
            }

            _context.Guests.AddRange(guestList);
            await _context.SaveChangesAsync();

            @event.Planner = planner; // Include planner for the response
            return @event;
        }

        public async Task<Event> UpdateEventAsync(int eventId, EventUpdateDto eventDto)
        {
            var @event = await _context.Events
                .Include(e => e.Planner)
                .FirstOrDefaultAsync(e => e.Id == eventId)
                ?? throw new ArgumentException("Event not found.");

            @event.EventName = eventDto.EventName ?? @event.EventName;
            @event.Location = eventDto.Location ?? @event.Location;
            @event.Date = eventDto.Date != default ? eventDto.Date : @event.Date;
            @event.Time = eventDto.Time ?? @event.Time;
            @event.Description = eventDto.Description ?? @event.Description;
            @event.NumberOfGuests = eventDto.NumberOfGuests != 0 ? eventDto.NumberOfGuests : @event.NumberOfGuests;
            @event.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task DeleteEventAsync(int eventId)
        {
            var @event = await _context.Events.FindAsync(eventId) ?? throw new ArgumentException("Event not found.");

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
        }

        public async Task<Event> GetEventByIdAsync(int eventId)
        {
            var @event = await _context.Events
                .Include(e => e.Planner)
                .Include(e => e.Guests) // Include guests for other methods
                .FirstOrDefaultAsync(e => e.Id == eventId)
                ?? throw new ArgumentException("Event not found.");
            return @event;
        }

        public async Task<List<Event>> GetEventsByPlannerAsync(int plannerId)
        {
            return await _context.Events
                .Include(e => e.Planner)
                .Where(e => e.PlannerId == plannerId)
                .ToListAsync();
        }

        public async Task<List<Event>> GetUpcomingEventsAsync(int plannerId)
        {
            return await _context.Events
                .Include(e => e.Planner)
                .Where(e => e.PlannerId == plannerId &&
                            e.Date > DateTime.UtcNow.AddHours(-24) &&
                            e.Status != EventStatus.Completed)
                .ToListAsync();
        }

        public async Task<List<Guest>> AddGuestsAsync(int eventId, List<GuestDto> guests)
        {
            var @event = await _context.Events.FindAsync(eventId) ?? throw new ArgumentException("Event not found.");

            if (string.IsNullOrEmpty(@event.EventName))
                throw new ArgumentException("Event name cannot be null or empty.");

            var guestList = new List<Guest>();
            foreach (var guestDto in guests)
            {
                if (string.IsNullOrEmpty(guestDto.Name))
                    throw new ArgumentException("Guest name cannot be null or empty.");

                var qrCodeDto = new GenerateQrCodeDto
                {
                    EventId = eventId,
                    GuestName = guestDto.Name,
                    GuestEmail = guestDto.Email
                };
                var qrCode = await _qrCodeService.GenerateQrCodeAsync(qrCodeDto);

                var guest = new Guest
                {
                    EventId = eventId,
                    Name = guestDto.Name,
                    Email = guestDto.Email,
                    QrCode = qrCode.QrCodeValue,
                    Status = GuestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                guestList.Add(guest);
            }

            _context.Guests.AddRange(guestList);
            await _context.SaveChangesAsync();
            return guestList;
        }

        public async Task MarkGuestsAbsentAsync(int eventId)
        {
            var guests = await _context.Guests
                .Where(g => g.EventId == eventId && g.Status == GuestStatus.Pending)
                .ToListAsync();

            foreach (var guest in guests)
            {
                guest.Status = GuestStatus.Absent;
                guest.UpdatedAt = DateTime.UtcNow;
            }

            var @event = await _context.Events.FindAsync(eventId);
            if (@event != null)
            {
                @event.Status = EventStatus.Completed;
                @event.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RegenerateQrCodesForEventAsync(int eventId)
        {
            var @event = await _context.Events
                .Include(e => e.Guests)
                .FirstOrDefaultAsync(e => e.Id == eventId)
                ?? throw new ArgumentException("Event not found.");

            // Remove existing QR codes
            var existingQrCodes = await _context.QrCodes
                .Where(q => q.EventId == eventId)
                .ToListAsync();

            if (existingQrCodes.Any())
            {
                _context.QrCodes.RemoveRange(existingQrCodes);
                await _context.SaveChangesAsync();
            }

            // Generate new QR codes for each guest
            foreach (var guest in @event.Guests)
            {
                var qrCodeDto = new GenerateQrCodeDto
                {
                    EventId = @event.Id,
                    GuestName = guest.Name,
                    GuestEmail = guest.Email
                };
                var qrCode = await _qrCodeService.GenerateQrCodeAsync(qrCodeDto);
                guest.QrCode = qrCode.QrCodeValue;
                guest.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Guest>> GetGuestsByEventIdAsync(int eventId)
        {
            var @event = await _context.Events
                .Include(e => e.Guests)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null)
            {
                throw new ArgumentException($"Event with ID {eventId} not found.");
            }

            return @event.Guests?.ToList() ?? new List<Guest>();
        }

        public async Task<List<QrCode>> GetQrCodesByEventIdAsync(int eventId)
        {
            var qrCodes = await _context.QrCodes
                .Where(q => q.EventId == eventId)
                .ToListAsync();

            return qrCodes ?? new List<QrCode>();
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Planner)
                .ToListAsync();
        }
    }
}