using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using EventManager.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Planner")]
    public class GuestController : ControllerBase
    {
        private readonly IQrCodeService _qrCodeService;
        private readonly IValidator<ScanQrCodeDto> _qrCodeValidator;

        public GuestController(IQrCodeService qrCodeService, IValidator<ScanQrCodeDto> qrCodeValidator)
        {
            _qrCodeService = qrCodeService;
            _qrCodeValidator = qrCodeValidator;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanQrCode([FromBody] ScanQrCodeDto scanDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Invalid user ID.");

                // Temporarily bypass validation to test
                // var validationResult = await _qrCodeValidator.ValidateAsync(scanDto);
                // if (!validationResult.IsValid)
                // {
                //     var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                //     return BadRequest($"Validation failed: {errors}");
                // }

                var guest = await _qrCodeService.ScanQrCodeAsync(scanDto, User);
                return Ok(guest);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning QR code: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while scanning the QR code.");
            }
        }
    }
}