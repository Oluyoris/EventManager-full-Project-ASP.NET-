using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Planner")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto eventDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.CreateEventAsync(userId, eventDto);
                var response = new EventResponseDto
                {
                    Id = @event.Id,
                    EventName = @event.EventName,
                    Location = @event.Location,
                    Date = @event.Date,
                    Time = @event.Time,
                    Description = @event.Description,
                    NumberOfGuests = @event.NumberOfGuests,
                    PlannerId = @event.PlannerId,
                    PlannerName = @event.Planner?.FullName ?? @event.Planner?.Username,
                    Status = @event.Status,
                    CreatedAt = @event.CreatedAt,
                    UpdatedAt = @event.UpdatedAt
                };
                return CreatedAtAction(nameof(GetEvent), new { eventId = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating event: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while creating the event.");
            }
        }

        [HttpPut("{eventId}")]
        public async Task<IActionResult> UpdateEvent(int eventId, [FromBody] EventUpdateDto eventDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.GetEventByIdAsync(eventId);
                if (@event == null)
                    return NotFound($"Event with ID {eventId} not found.");

                if (@event.PlannerId != userId)
                    return Forbid("You are not authorized to update this event.");

                var updatedEvent = await _eventService.UpdateEventAsync(eventId, eventDto);
                var response = new EventResponseDto
                {
                    Id = updatedEvent.Id,
                    EventName = updatedEvent.EventName,
                    Location = updatedEvent.Location,
                    Date = updatedEvent.Date,
                    Time = updatedEvent.Time,
                    Description = updatedEvent.Description,
                    NumberOfGuests = updatedEvent.NumberOfGuests,
                    PlannerId = updatedEvent.PlannerId,
                    PlannerName = updatedEvent.Planner?.FullName ?? updatedEvent.Planner?.Username,
                    Status = updatedEvent.Status,
                    CreatedAt = updatedEvent.CreatedAt,
                    UpdatedAt = updatedEvent.UpdatedAt
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating event with ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while updating the event.");
            }
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.GetEventByIdAsync(eventId);
                if (@event == null)
                    return NotFound($"Event with ID {eventId} not found.");

                if (@event.PlannerId != userId)
                    return Forbid("You are not authorized to delete this event.");

                await _eventService.DeleteEventAsync(eventId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting event with ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while deleting the event.");
            }
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetEvent(int eventId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.GetEventByIdAsync(eventId);
                if (@event == null)
                    return NotFound($"Event with ID {eventId} not found.");

                if (@event.PlannerId != userId)
                    return Forbid("You are not authorized to view this event.");

                var guests = await _eventService.GetGuestsByEventIdAsync(eventId);
                var qrCodes = await _eventService.GetQrCodesByEventIdAsync(eventId);

                var eventResponse = new EventResponseDto
                {
                    Id = @event.Id,
                    EventName = @event.EventName,
                    Location = @event.Location,
                    Date = @event.Date,
                    Time = @event.Time,
                    Description = @event.Description,
                    NumberOfGuests = @event.NumberOfGuests,
                    PlannerId = @event.PlannerId,
                    PlannerName = @event.Planner?.FullName ?? @event.Planner?.Username,
                    Status = @event.Status,
                    CreatedAt = @event.CreatedAt,
                    UpdatedAt = @event.UpdatedAt
                };

                var guestResponses = guests.Select(g => new GuestResponseDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Email = g.Email,
                    Status = g.Status.ToString(),
                    QrCode = g.QrCode
                }).ToList();

                var qrCodeResponses = qrCodes.Select(q => new QrCodeResponseDto
                {
                    Id = q.Id,
                    QrCodeValue = q.QrCodeValue
                }).ToList();

                var response = new EventDetailsResponseDto
                {
                    Event = eventResponse,
                    Guests = guestResponses,
                    QrCodes = qrCodeResponses
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event with ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while fetching the event.");
            }
        }

        [HttpGet("my-events")]
        public async Task<IActionResult> GetMyEvents()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var events = await _eventService.GetEventsByPlannerAsync(userId);
                var response = events.Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    EventName = e.EventName,
                    Location = e.Location,
                    Date = e.Date,
                    Time = e.Time,
                    Description = e.Description,
                    NumberOfGuests = e.NumberOfGuests,
                    PlannerId = e.PlannerId,
                    PlannerName = e.Planner?.FullName ?? e.Planner?.Username,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList();
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching events for planner: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while fetching events.");
            }
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var events = await _eventService.GetUpcomingEventsAsync(userId);
                var response = events.Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    EventName = e.EventName,
                    Location = e.Location,
                    Date = e.Date,
                    Time = e.Time,
                    Description = e.Description,
                    NumberOfGuests = e.NumberOfGuests,
                    PlannerId = e.PlannerId,
                    PlannerName = e.Planner?.FullName ?? e.Planner?.Username,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList();
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching upcoming events: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while fetching upcoming events.");
            }
        }

        [HttpPost("{eventId}/guests")]
        public async Task<IActionResult> AddGuests(int eventId, [FromBody] List<GuestDto> guests)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.GetEventByIdAsync(eventId);
                if (@event == null)
                    return NotFound($"Event with ID {eventId} not found.");

                if (@event.PlannerId != userId)
                    return Forbid("You are not authorized to add guests to this event.");

                var addedGuests = await _eventService.AddGuestsAsync(eventId, guests);
                return Ok(addedGuests);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding guests to event with ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while adding guests.");
            }
        }

        [HttpPost("{eventId}/mark-absent")]
        public async Task<IActionResult> MarkGuestsAbsent(int eventId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.GetEventByIdAsync(eventId);
                if (@event == null)
                    return NotFound($"Event with ID {eventId} not found.");

                if (@event.PlannerId != userId)
                    return Forbid("You are not authorized to mark guests absent for this event.");

                await _eventService.MarkGuestsAbsentAsync(eventId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking guests absent for event with ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while marking guests absent.");
            }
        }

        [HttpPost("{eventId}/regenerate-qrcodes")]
        public async Task<IActionResult> RegenerateQrCodes(int eventId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.GetEventByIdAsync(eventId);
                if (@event == null)
                    return NotFound($"Event with ID {eventId} not found.");

                if (@event.PlannerId != userId)
                    return Forbid("You are not authorized to regenerate QR codes for this event.");

                await _eventService.RegenerateQrCodesForEventAsync(eventId);
                return Ok("QR codes regenerated successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error regenerating QR codes for event with ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while regenerating QR codes.");
            }
        }
    }
}