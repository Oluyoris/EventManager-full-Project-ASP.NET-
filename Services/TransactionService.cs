using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EventManager.Api.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly EventManagerDbContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(EventManagerDbContext context, ILogger<TransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TransactionDto> ApprovePaymentAsync(int adminId, ApprovePaymentDto approveDto)
        {
            _logger.LogInformation("Approving transaction ID: {TransactionId} by admin ID: {AdminId}, IsApproved: {IsApproved}", approveDto.TransactionId, adminId, approveDto.IsApproved);
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                _logger.LogError("Admin not found for ID: {AdminId}", adminId);
                throw new ArgumentException("Admin not found.");
            }

            var transaction = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == approveDto.TransactionId);
            if (transaction == null)
            {
                _logger.LogError("Transaction not found for ID: {TransactionId}", approveDto.TransactionId);
                throw new ArgumentException("Transaction not found.");
            }

            if (transaction.User == null)
            {
                _logger.LogError("Transaction user not found for transaction ID: {TransactionId}, UserId: {UserId}", approveDto.TransactionId, transaction.UserId);
                throw new ArgumentException("Transaction user not found.");
            }

            transaction.Status = approveDto.IsApproved ? TransactionStatus.Completed : TransactionStatus.Failed;
            transaction.UpdatedAt = DateTime.UtcNow;

            if (approveDto.IsApproved)
            {
                transaction.User.Balance += transaction.Amount;
                _logger.LogInformation("Updated user balance for user ID: {UserId} to {Balance}", transaction.User.Id, transaction.User.Balance);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Transaction ID: {TransactionId} updated successfully.", transaction.Id);

            return new TransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                EventId = transaction.EventId,
                Amount = transaction.Amount,
                Gateway = transaction.Gateway.ToString(),
                Status = transaction.Status.ToString(),
                Reference = transaction.Reference,
                ProofFilePath = transaction.ProofFilePath,
                PaymentUrl = transaction.PaymentUrl,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                UserFullName = transaction.User.FullName,
                UserUsername = transaction.User.Username
            };
        }

        public async Task<List<TransactionDto>> GetTransactionsByPlannerAsync(int plannerId)
        {
            _logger.LogInformation("Fetching transactions for planner ID: {PlannerId}", plannerId);
            var transactions = await _context.Transactions
                .Include(t => t.Event)
                .Include(t => t.User)
                .Where(t => t.UserId == plannerId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    EventId = t.EventId,
                    Amount = t.Amount,
                    Gateway = t.Gateway.ToString(),
                    Status = t.Status.ToString(),
                    Reference = t.Reference,
                    ProofFilePath = t.ProofFilePath,
                    PaymentUrl = t.PaymentUrl,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    UserFullName = t.User != null ? t.User.FullName : "Unknown",
                    UserUsername = t.User != null ? t.User.Username : "Unknown"
                })
                .ToListAsync();

            if (transactions.Any(t => t.UserFullName == "Unknown"))
            {
                _logger.LogWarning("Null users for transactions: {TransactionIds}, UserId: {UserId}",
                    string.Join(", ", transactions.Where(t => t.UserFullName == "Unknown").Select(t => t.Id)),
                    plannerId);
            }

            _logger.LogInformation("Fetched {Count} transactions for planner ID: {PlannerId}", transactions.Count, plannerId);
            return transactions;
        }

        public async Task<List<TransactionDto>> GetAllTransactionsAsync()
        {
            _logger.LogInformation("Fetching all transactions.");
            var transactions = await _context.Transactions
                .Include(t => t.Event)
                .Include(t => t.User)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    EventId = t.EventId,
                    Amount = t.Amount,
                    Gateway = t.Gateway.ToString(),
                    Status = t.Status.ToString(),
                    Reference = t.Reference,
                    ProofFilePath = t.ProofFilePath,
                    PaymentUrl = t.PaymentUrl,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    UserFullName = t.User != null ? t.User.FullName : "Unknown",
                    UserUsername = t.User != null ? t.User.Username : "Unknown"
                })
                .ToListAsync();

            if (transactions.Any(t => t.UserFullName == "Unknown"))
            {
                _logger.LogWarning("Null users for transactions: {TransactionIds}",
                    string.Join(", ", transactions.Where(t => t.UserFullName == "Unknown").Select(t => t.Id)));
            }

            _logger.LogInformation("Fetched {Count} transactions.", transactions.Count);
            return transactions;
        }

        public async Task<List<TransactionDto>> GetPendingTransactionsAsync()
        {
            _logger.LogInformation("Fetching pending transactions.");
            var transactions = await _context.Transactions
                .Include(t => t.User)
                .Where(t => t.Status == TransactionStatus.Pending)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    EventId = t.EventId,
                    Amount = t.Amount,
                    Gateway = t.Gateway.ToString(),
                    Status = t.Status.ToString(),
                    Reference = t.Reference,
                    ProofFilePath = t.ProofFilePath,
                    PaymentUrl = t.PaymentUrl,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    UserFullName = t.User != null ? t.User.FullName : "Unknown",
                    UserUsername = t.User != null ? t.User.Username : "Unknown"
                })
                .ToListAsync();

            if (transactions.Any(t => t.UserFullName == "Unknown"))
            {
                _logger.LogWarning("Null users for pending transactions: {TransactionIds}",
                    string.Join(", ", transactions.Where(t => t.UserFullName == "Unknown").Select(t => t.Id)));
            }

            _logger.LogInformation("Fetched {Count} pending transactions.", transactions.Count);
            return transactions;
        }

        public async Task<List<TransactionDto>> GetApprovedTransactionsAsync()
        {
            _logger.LogInformation("Fetching approved transactions.");
            var transactions = await _context.Transactions
                .Include(t => t.User)
                .Where(t => t.Status == TransactionStatus.Completed)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    EventId = t.EventId,
                    Amount = t.Amount,
                    Gateway = t.Gateway.ToString(),
                    Status = t.Status.ToString(),
                    Reference = t.Reference,
                    ProofFilePath = t.ProofFilePath,
                    PaymentUrl = t.PaymentUrl,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    UserFullName = t.User != null ? t.User.FullName : "Unknown",
                    UserUsername = t.User != null ? t.User.Username : "Unknown"
                })
                .ToListAsync();

            if (transactions.Any(t => t.UserFullName == "Unknown"))
            {
                _logger.LogWarning("Null users for approved transactions: {TransactionIds}",
                    string.Join(", ", transactions.Where(t => t.UserFullName == "Unknown").Select(t => t.Id)));
            }

            _logger.LogInformation("Fetched {Count} approved transactions.", transactions.Count);
            return transactions;
        }

        public async Task<List<TransactionDto>> GetRejectedTransactionsAsync()
        {
            _logger.LogInformation("Fetching rejected transactions.");
            var transactions = await _context.Transactions
                .Include(t => t.User)
                .Where(t => t.Status == TransactionStatus.Failed)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    EventId = t.EventId,
                    Amount = t.Amount,
                    Gateway = t.Gateway.ToString(),
                    Status = t.Status.ToString(),
                    Reference = t.Reference,
                    ProofFilePath = t.ProofFilePath,
                    PaymentUrl = t.PaymentUrl,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    UserFullName = t.User != null ? t.User.FullName : "Unknown",
                    UserUsername = t.User != null ? t.User.Username : "Unknown"
                })
                .ToListAsync();

            if (transactions.Any(t => t.UserFullName == "Unknown"))
            {
                _logger.LogWarning("Null users for rejected transactions: {TransactionIds}",
                    string.Join(", ", transactions.Where(t => t.UserFullName == "Unknown").Select(t => t.Id)));
            }

            _logger.LogInformation("Fetched {Count} rejected transactions.", transactions.Count);
            return transactions;
        }
    }
}