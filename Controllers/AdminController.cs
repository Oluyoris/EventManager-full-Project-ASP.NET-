using EventManager.Api.Dtos;
using EventManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IEventService _eventService;

        public AdminController(IAdminService adminService, IEventService eventService)
        {
            _adminService = adminService;
            _eventService = eventService;
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto updateDto)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var user = await _adminService.UpdateProfileAsync(adminId, updateDto);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("profile/password")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateDto passwordDto)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                await _adminService.UpdatePasswordAsync(adminId, passwordDto);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var user = await _adminService.GetProfileAsync(adminId);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("planners")]
        public async Task<IActionResult> GetAllPlanners()
        {
            var planners = await _adminService.GetAllPlannersAsync();
            return Ok(planners);
        }

        [HttpGet("planners/{plannerId}")]
        public async Task<IActionResult> GetPlannerById(int plannerId)
        {
            try
            {
                var planner = await _adminService.GetPlannerByIdAsync(plannerId);
                var result = new
                {
                    planner.Id,
                    planner.FullName,
                    planner.Username,
                    planner.Email,
                    Balance = planner.Balance,
                    planner.PhoneNumber,
                    planner.CountryCode,
                    planner.Country,
                    planner.CompanyName,
                    planner.Role,
                    planner.IsBlocked,
                    planner.IsEmailVerified,
                    planner.CreatedAt,
                    planner.UpdatedAt,
                    planner.Events,
                    planner.Transactions
                };
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("planners/{plannerId}/block")]
        public async Task<IActionResult> BlockPlanner(int plannerId)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var planner = await _adminService.BlockPlannerAsync(adminId, plannerId);
                return Ok(planner);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("planners/{plannerId}")]
        public async Task<IActionResult> DeletePlanner(int plannerId)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var planner = await _adminService.DeletePlannerAsync(adminId, plannerId);
                return Ok(planner);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("planners/{plannerId}/events")]
        public async Task<IActionResult> CreateEventForPlanner(int plannerId, [FromBody] EventCreateDto eventDto)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _adminService.CreateEventForPlannerAsync(adminId, plannerId, eventDto);
                return CreatedAtAction(nameof(CreateEventForPlanner), new { plannerId, eventId = @event.Id }, @event);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("planners/{plannerId}/balance")]
        public async Task<IActionResult> UpdatePlannerBalance(int plannerId, [FromBody] UpdateBalanceDto balanceDto)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var planner = await _adminService.UpdatePlannerBalanceAsync(adminId, plannerId, balanceDto.Amount);
                return Ok(planner);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardAnalytics()
        {
            var analytics = await _adminService.GetDashboardAnalyticsAsync();
            return Ok(analytics);
        }

        [HttpGet("payment-settings")]
        public async Task<IActionResult> GetPaymentSettings()
        {
            try
            {
                var settings = await _adminService.GetPaymentSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("payment-settings")]
        public async Task<IActionResult> UpdatePaymentSettings([FromBody] PaymentMethodUpdateDto updateDto)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var settings = await _adminService.UpdatePaymentSettingsAsync(adminId, updateDto);
                return Ok(settings);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("notifications/pending-transactions-count")]
        public async Task<IActionResult> GetPendingTransactionsCount()
        {
            try
            {
                var count = await _adminService.GetPendingTransactionsCountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events = await _eventService.GetAllEventsAsync();
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("events/{eventId}")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var @event = await _eventService.GetEventByIdAsync(eventId);
                if (@event == null)
                    return NotFound($"Event with ID {eventId} not found.");

                await _eventService.DeleteEventAsync(eventId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}