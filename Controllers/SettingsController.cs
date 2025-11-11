using EventManager.Api.Dtos;
using EventManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EventManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        [HttpPut("update-payment-method")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentMethod([FromBody] PaymentMethodUpdateDto paymentMethodDto)
        {
            _logger.LogDebug("UpdatePaymentMethod called");
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var paymentMethod = await _settingsService.UpdatePaymentMethodAsync(adminId, paymentMethodDto);
                return Ok(paymentMethod);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error in UpdatePaymentMethod");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("site")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSiteSettings([FromBody] SiteSettingsUpdateDto settingsDto)
        {
            _logger.LogDebug("UpdateSiteSettings called");
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized("Invalid user ID.");

                var siteSettings = await _settingsService.UpdateSiteSettingsAsync(adminId, settingsDto);
                return Ok(siteSettings);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error in UpdateSiteSettings");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-payment-method")]
        [Authorize(Roles = "Planner,Admin")]
        public async Task<IActionResult> GetPaymentMethod()
        {
            _logger.LogDebug("GetPaymentMethod called");
            try
            {
                var paymentMethod = await _settingsService.GetPaymentMethodAsync();
                _logger.LogDebug("Payment method retrieved: {@PaymentMethod}", paymentMethod);
                return Ok(paymentMethod);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error in GetPaymentMethod");
                return NotFound(ex.Message); // Return 404 if payment method is not configured
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetPaymentMethod");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("pricing")]
        [Authorize(Roles = "Planner,Admin")]
        public async Task<IActionResult> GetPricingSettings()
        {
            _logger.LogDebug("GetPricingSettings called");
            try
            {
                var siteSettings = await _settingsService.GetSiteSettingsAsync();
                if (siteSettings == null)
                {
                    _logger.LogWarning("Site settings not found.");
                    return NotFound("Site settings not found.");
                }

                var pricingSettings = new
                {
                    EventPricePerGuest = siteSettings.EventPricePerGuest,
                    GuestDetailsFee = siteSettings.GuestDetailsFee,
                    Currency = siteSettings.Currency
                };

                _logger.LogDebug("Pricing settings retrieved: {@PricingSettings}", pricingSettings);
                return Ok(pricingSettings);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error in GetPricingSettings");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetPricingSettings");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}