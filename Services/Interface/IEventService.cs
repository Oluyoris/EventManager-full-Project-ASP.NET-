using EventManager.Api.Dtos;
using EventManager.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(int plannerId, EventCreateDto eventDto);
        Task<Event> UpdateEventAsync(int eventId, EventUpdateDto eventDto);
        Task DeleteEventAsync(int eventId);
        Task<Event> GetEventByIdAsync(int eventId);
        Task<List<Event>> GetEventsByPlannerAsync(int plannerId);
        Task<List<Event>> GetUpcomingEventsAsync(int plannerId);
        Task<List<Guest>> AddGuestsAsync(int eventId, List<GuestDto> guests);
        Task MarkGuestsAbsentAsync(int eventId);
        Task RegenerateQrCodesForEventAsync(int eventId);
        Task<List<Guest>> GetGuestsByEventIdAsync(int eventId);
        Task<List<QrCode>> GetQrCodesByEventIdAsync(int eventId);
        Task<List<Event>> GetAllEventsAsync();
    }
}
