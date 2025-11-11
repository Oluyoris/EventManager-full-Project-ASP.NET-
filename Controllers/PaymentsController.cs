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
    [Authorize(Roles = "Planner")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("manual")]
        public async Task<IActionResult> SubmitManualPayment([FromBody] ManualPaymentDto paymentDto)
        {
            try
            {
                _logger.LogInformation("Received manual payment request. Amount: {Amount}, ProofFileName: {ProofFileName}",
                    paymentDto.Amount, paymentDto.ProofFileName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID claim: {ClaimValue}", userIdClaim);
                    return Unauthorized(new { message = "Invalid user ID." });
                }

                if (string.IsNullOrEmpty(paymentDto.ProofFileBase64))
                {
                    _logger.LogWarning("Proof file is missing or empty.");
                    return BadRequest(new { message = "Proof file is required." });
                }

                var allowedTypes = new[] { "application/pdf", "image/png", "image/jpeg" };
                if (!allowedTypes.Contains(paymentDto.ContentType))
                {
                    _logger.LogWarning("Invalid proof file type: {ContentType}", paymentDto.ContentType);
                    return BadRequest(new { message = "Proof file must be a PDF, PNG, or JPEG." });
                }

                var response = await _paymentService.SubmitManualPaymentAsync(userId, paymentDto);
                if (response?.Transaction != null)
                {
                    _logger.LogInformation("Manual payment submitted successfully. Transaction ID: {TransactionId}", response.Transaction.Id);
                }
                else
                {
                    _logger.LogWarning("Manual payment response or transaction is null.");
                }
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error in SubmitManualPayment: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SubmitManualPayment: {Message}", ex.Message);
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("automatic")]
        public async Task<IActionResult> InitiateAutomaticPayment([FromBody] AutomaticPaymentDto paymentDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID claim: {ClaimValue}", userIdClaim);
                    return Unauthorized(new { message = "Invalid user ID." });
                }

                var response = await _paymentService.InitiateAutomaticPaymentAsync(userId, paymentDto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error in InitiateAutomaticPayment: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in InitiateAutomaticPayment: {Message}", ex.Message);
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDto verifyDto)
        {
            try
            {
                var transaction = await _paymentService.VerifyAutomaticPaymentAsync(verifyDto);
                return Ok(transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error in VerifyPayment: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in VerifyPayment: {Message}", ex.Message);
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID claim: {ClaimValue}", userIdClaim);
                    return Unauthorized(new { message = "Invalid user ID." });
                }

                var balance = await _paymentService.GetPlannerBalanceAsync(userId);
                return Ok(new { Balance = balance });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error in GetBalance: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetBalance: {Message}", ex.Message);
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}