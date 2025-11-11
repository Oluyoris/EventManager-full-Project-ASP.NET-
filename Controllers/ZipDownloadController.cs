using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EventManager.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace EventManager.Api.Controllers
{
    [Route("api/zipdownload")] // Simple base route
    [ApiController]
    [Authorize(Roles = "Planner")]
    public class ZipDownloadController : ControllerBase
    {
        private readonly EventManagerDbContext _context;

        public ZipDownloadController(EventManagerDbContext context)
        {
            _context = context;
        }

        [HttpGet("event/{eventId}/qrcodes/zip")]
        public async Task<IActionResult> DownloadQrCodesAsZip(int eventId)
        {
            try
            {
                Console.WriteLine($"Received request to download QR codes ZIP for event ID: {eventId}");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    Console.WriteLine("Invalid user ID in token.");
                    return Unauthorized("Invalid user ID.");
                }

                var @event = await _context.Events.FindAsync(eventId);
                if (@event == null)
                {
                    Console.WriteLine($"Event with ID {eventId} not found.");
                    return NotFound($"Event with ID {eventId} not found.");
                }

                if (@event.PlannerId != userId)
                {
                    Console.WriteLine($"User {userId} is not authorized to access event {eventId}.");
                    return Forbid("You are not authorized to access QR codes for this event.");
                }

                var qrCodes = await _context.QrCodes
                    .Where(q => q.EventId == eventId)
                    .ToListAsync();

                if (qrCodes == null || !qrCodes.Any())
                {
                    Console.WriteLine($"No QR codes found for event ID {eventId}.");
                    return NotFound("No QR codes found for this event.");
                }

                Console.WriteLine($"Found {qrCodes.Count} QR codes for event ID {eventId}.");

                using (var memoryStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var qrCode in qrCodes)
                        {
                            if (qrCode.QrCodeImage != null)
                            {
                                Console.WriteLine($"Adding QR code {qrCode.QrCodeValue}.png to ZIP.");
                                var entry = zipArchive.CreateEntry($"{qrCode.QrCodeValue}.png");
                                using (var entryStream = entry.Open())
                                {
                                    await entryStream.WriteAsync(qrCode.QrCodeImage, 0, qrCode.QrCodeImage.Length);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"QR code {qrCode.QrCodeValue} has no image data.");
                            }
                        }
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Console.WriteLine($"Returning ZIP file for event ID {eventId}, size: {memoryStream.Length} bytes.");
                    return File(memoryStream.ToArray(), "application/zip", $"Event_{eventId}_QRCodes.zip");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating QR code ZIP for event ID {eventId}: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while generating the QR code ZIP file.");
            }
        }
    }
}