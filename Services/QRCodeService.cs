using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ZXing;
using ZXing.QrCode;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using ZXing.SkiaSharp;
using ZXing.SkiaSharp.Rendering;
using System.Security.Claims;

namespace EventManager.Api.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly EventManagerDbContext _context;

        public QrCodeService(EventManagerDbContext context)
        {
            _context = context;
        }

        public async Task<QrCode> GenerateQrCodeAsync(GenerateQrCodeDto qrCodeDto)
        {
            try
            {
                var @event = await _context.Events.FindAsync(qrCodeDto.EventId) ?? throw new ArgumentException("Event not found.");

                var eventNamePrefix = (@event.EventName?.Length >= 4 ? @event.EventName.Substring(0, 4) : @event.EventName != null ? @event.EventName : "EVNT").ToUpper();
                var qrCodeValue = $"{eventNamePrefix}-{(await _context.QrCodes.CountAsync(q => q.EventId == qrCodeDto.EventId) + 1).ToString("D4")}";
                var qrCode = new QrCode
                {
                    EventId = qrCodeDto.EventId,
                    GuestName = qrCodeDto.GuestName,
                    QrCodeValue = qrCodeValue,
                    CreatedAt = DateTime.UtcNow
                };

                // Log the QR code value being generated
                Console.WriteLine($"Generating QR code for EventId: {qrCodeDto.EventId}, GuestName: {qrCodeDto.GuestName}, Value: {qrCodeValue}");

                // Generate QR code image with ZXing.Net and SkiaSharp
                var writer = new BarcodeWriter<SKBitmap>
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Width = 500,
                        Height = 500,
                        Margin = 1,
                        ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q
                    },
                    Renderer = new SKBitmapRenderer()
                };

                // Generate the QR code as an SKBitmap
                using var qrCodeBitmap = await Task.Run(() => writer.Write(qrCodeValue));
                using var stream = new MemoryStream();
                qrCodeBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                qrCode.QrCodeImage = stream.ToArray();

                _context.QrCodes.Add(qrCode);
                await _context.SaveChangesAsync();

                Console.WriteLine($"QR code generated and saved for EventId: {qrCodeDto.EventId}, GuestName: {qrCodeDto.GuestName}");
                return qrCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating QR code for EventId: {qrCodeDto.EventId}, GuestName: {qrCodeDto.GuestName}. Error: {ex.Message}");
                throw new InvalidOperationException($"Failed to generate QR code: {ex.Message}", ex);
            }
        }

        public async Task<Guest> ScanQrCodeAsync(ScanQrCodeDto scanDto, ClaimsPrincipal? user = null)
        {
            try
            {
                // Validate planner authorization if user is provided (from controller)
                if (user != null)
                {
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!int.TryParse(userIdClaim, out int userId))
                    {
                        throw new ArgumentException("Invalid user ID in token.");
                    }

                    var @event = await _context.Events.FindAsync(scanDto.EventId);
                    if (@event == null)
                    {
                        throw new ArgumentException($"Event with ID {scanDto.EventId} not found.");
                    }

                    if (@event.PlannerId != userId)
                    {
                        throw new ArgumentException("You are not authorized to scan QR codes for this event.");
                    }
                }

                // Validate QR code
                var qrCode = await _context.QrCodes
                    .Include(q => q.Event)
                    .FirstOrDefaultAsync(q => q.QrCodeValue == scanDto.QrCodeValue && q.EventId == scanDto.EventId);
                if (qrCode == null)
                {
                    throw new ArgumentException($"Invalid QR code: {scanDto.QrCodeValue} does not belong to event ID {scanDto.EventId}.");
                }

                // Find the associated guest
                var guest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.EventId == qrCode.EventId && g.Name == qrCode.GuestName);
                if (guest == null)
                {
                    throw new ArgumentException($"Guest with name {qrCode.GuestName} not found for event ID {qrCode.EventId}.");
                }

                // Check if guest is already marked as present
                if (guest.Status == GuestStatus.Present)
                {
                    throw new ArgumentException($"Guest {guest.Name} is already marked as present.");
                }

                // Update guest status to "Present"
                guest.Status = GuestStatus.Present;
                guest.UpdatedAt = DateTime.UtcNow;

                // Check if all guests are present, and update event status if so
                var allGuests = await _context.Guests
                    .Where(g => g.EventId == qrCode.EventId)
                    .ToListAsync();
                if (allGuests.All(g => g.Status == GuestStatus.Present))
                {
                    var eventToUpdate = await _context.Events.FindAsync(qrCode.EventId);
                    if (eventToUpdate != null)
                    {
                        eventToUpdate.Status = EventStatus.Completed;
                        eventToUpdate.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Guest {guest.Name} (ID: {guest.Id}) marked as Present for Event ID {qrCode.EventId}.");
                return guest;
            }
            catch (ArgumentException)
            {
                throw; // Re-throw to let the controller handle it as a BadRequest
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning QR code {scanDto.QrCodeValue} for Event ID {scanDto.EventId}: {ex.Message}");
                throw new InvalidOperationException($"Failed to scan QR code: {ex.Message}", ex);
            }
        }

        public async Task<QrCode> GetQrCodeByIdAsync(int qrCodeId)
        {
            var qrCode = await _context.QrCodes.FindAsync(qrCodeId);
            if (qrCode == null)
            {
                throw new ArgumentException($"QR code with ID {qrCodeId} not found.");
            }
            return qrCode;
        }
    }
}