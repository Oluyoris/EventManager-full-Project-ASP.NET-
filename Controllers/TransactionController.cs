using EventManager.Api.Dtos;
using EventManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore; // Added for direct DB access

namespace EventManager.Api.Controllers
{
    [ApiController]
    [Route("api/admin/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly EventManager.Api.Data.EventManagerDbContext _context; // Added for DB access

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger, IWebHostEnvironment environment, EventManager.Api.Data.EventManagerDbContext context)
        {
            _transactionService = transactionService;
            _logger = logger;
            _environment = environment;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                _logger.LogInformation("Fetching all transactions for admin.");
                var transactions = await _transactionService.GetAllTransactionsAsync();
                _logger.LogInformation("Fetched {Count} transactions.", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all transactions.");
                return StatusCode(500, new { message = "An error occurred while fetching transactions." });
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingTransactions()
        {
            try
            {
                _logger.LogInformation("Fetching pending transactions.");
                var transactions = await _transactionService.GetPendingTransactionsAsync();
                _logger.LogInformation("Fetched {Count} pending transactions.", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending transactions.");
                return StatusCode(500, new { message = "An error occurred while fetching pending transactions." });
            }
        }

        [HttpGet("approved")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApprovedTransactions()
        {
            try
            {
                _logger.LogInformation("Fetching approved transactions.");
                var transactions = await _transactionService.GetApprovedTransactionsAsync();
                _logger.LogInformation("Fetched {Count} approved transactions.", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching approved transactions.");
                return StatusCode(500, new { message = "An error occurred while fetching approved transactions." });
            }
        }

        [HttpGet("rejected")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRejectedTransactions()
        {
            try
            {
                _logger.LogInformation("Fetching rejected transactions.");
                var transactions = await _transactionService.GetRejectedTransactionsAsync();
                _logger.LogInformation("Fetched {Count} rejected transactions.", transactions.Count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rejected transactions.");
                return StatusCode(500, new { message = "An error occurred while fetching rejected transactions." });
            }
        }

        [HttpPost("approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveTransaction([FromBody] ApprovePaymentDto approveDto)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                {
                    _logger.LogWarning("Invalid admin ID in token.");
                    return Unauthorized(new { message = "Invalid user ID." });
                }

                _logger.LogInformation("Approving transaction ID: {TransactionId} by admin ID: {AdminId}", approveDto.TransactionId, adminId);
                var transaction = await _transactionService.ApprovePaymentAsync(adminId, approveDto);
                _logger.LogInformation("Transaction ID: {TransactionId} approved successfully.", transaction.Id);
                return Ok(transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error in ApproveTransaction: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ApproveTransaction: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while approving the transaction." });
            }
        }

        [HttpGet("my-transactions")]
        [Authorize(Roles = "Planner")]
        public async Task<IActionResult> GetMyTransactions()
        {
            try
            {
                var plannerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(plannerIdClaim, out int plannerId))
                {
                    _logger.LogWarning("Invalid planner ID in token.");
                    return Unauthorized(new { message = "Invalid user ID." });
                }

                _logger.LogInformation("Fetching transactions for planner ID: {PlannerId}", plannerId);
                var transactions = await _transactionService.GetTransactionsByPlannerAsync(plannerId);
                _logger.LogInformation("Fetched {Count} transactions for planner ID: {PlannerId}", transactions.Count, plannerId);
                return Ok(transactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error in GetMyTransactions: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMyTransactions: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while fetching transactions." });
            }
        }

        [HttpGet("proof/{transactionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProofFile(int transactionId)
        {
            try
            {
                _logger.LogInformation("Fetching proof file for transaction ID: {TransactionId}", transactionId);
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction == null)
                {
                    _logger.LogError("Transaction not found for ID: {TransactionId}", transactionId);
                    return NotFound(new { message = "Transaction not found." });
                }

                if (string.IsNullOrEmpty(transaction.ProofFilePath))
                {
                    _logger.LogError("Proof file path is empty for transaction ID: {TransactionId}", transactionId);
                    return NotFound(new { message = "Proof file not found." });
                }

                // Construct the full file path
                var filePath = Path.Combine(_environment.WebRootPath, transaction.ProofFilePath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("Proof file not found at path: {FilePath} for transaction ID: {TransactionId}", filePath, transactionId);
                    return NotFound(new { message = "Proof file not found on server." });
                }

                // Determine the MIME type based on file extension
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var mimeType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "application/octet-stream"
                };

                // Read the file and return it
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                _logger.LogInformation("Serving proof file for transaction ID: {TransactionId}", transactionId);
                return File(fileStream, mimeType, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching proof file for transaction ID: {TransactionId}", transactionId);
                return StatusCode(500, new { message = "An error occurred while fetching the proof file." });
            }
        }
    }
}