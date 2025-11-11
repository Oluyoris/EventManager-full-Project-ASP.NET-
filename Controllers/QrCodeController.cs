using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Planner")]
    public class QrCodeController : ControllerBase
    {
        private readonly EventManagerDbContext _context;

        public QrCodeController(EventManagerDbContext context)
        {
            _context = context;
        }

        // Example endpoint: Get all QR codes for an event
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetQrCodesForEvent(int eventId)
        {
            try
            {
                var qrCodes = await _context.QrCodes
                    .Where(q => q.EventId == eventId)
                    .ToListAsync();

                if (qrCodes == null || !qrCodes.Any())
                {
                    return NotFound($"No QR codes found for event ID {eventId}.");
                }

                return Ok(qrCodes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving QR codes for event ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while retrieving QR codes.");
            }
        }

        // Add other QR code-related endpoints here if they exist in your project
        // For example, generating QR codes, deleting QR codes, etc.
    }
}