using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventManager.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly EventManagerDbContext _context;
        private readonly HttpClient _httpClient;

        public PaymentService(EventManagerDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<TransactionResponseDto> SubmitManualPaymentAsync(int plannerId, ManualPaymentDto paymentDto)
        {
            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
                throw new ArgumentException("Planner not found.");

            if (string.IsNullOrWhiteSpace(paymentDto.ProofFileBase64))
                throw new ArgumentException("Proof file data is required.");

            if (string.IsNullOrWhiteSpace(paymentDto.ProofFileName))
                throw new ArgumentException("Proof file name is required.");

            var fileBytes = Convert.FromBase64String(paymentDto.ProofFileBase64);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(paymentDto.ProofFileName);
            var filePath = Path.Combine("Uploads", "proofs", fileName);

            var directoryPath = Path.Combine("Uploads", "proofs");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await File.WriteAllBytesAsync(filePath, fileBytes);

            var transaction = new Transaction
            {
                UserId = plannerId,
                Amount = paymentDto.Amount,
                Gateway = TransactionGateway.Manual,
                Status = TransactionStatus.Pending,
                Reference = Guid.NewGuid().ToString(),
                ProofFilePath = filePath,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var transactionDto = new TransactionDto
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
                UpdatedAt = transaction.UpdatedAt
            };

            return new TransactionResponseDto
            {
                Message = "Payment submitted successfully.",
                Transaction = transactionDto
            };
        }

        public async Task<PaymentResponseDto> InitiateAutomaticPaymentAsync(int plannerId, AutomaticPaymentDto paymentDto)
        {
            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
                throw new ArgumentException("Planner not found.");

            if (string.IsNullOrWhiteSpace(paymentDto.Gateway) ||
                !Enum.TryParse<TransactionGateway>(paymentDto.Gateway, true, out var parsedGateway))
            {
                throw new ArgumentException("Invalid or missing payment gateway.");
            }

            var reference = Guid.NewGuid().ToString();
            var paymentUrl = $"https://gateway.example.com/pay/{reference}";

            var transaction = new Transaction
            {
                UserId = plannerId,
                Amount = paymentDto.Amount,
                Gateway = parsedGateway,
                Status = TransactionStatus.Pending,
                Reference = reference,
                PaymentUrl = paymentUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new PaymentResponseDto
            {
                Reference = reference,
                PaymentUrl = paymentUrl,
                Status = "Pending"
            };
        }

        public async Task<TransactionDto> VerifyAutomaticPaymentAsync(VerifyPaymentDto verifyDto)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Reference == verifyDto.Reference);

            if (transaction == null)
                throw new ArgumentException("Transaction not found.");

            transaction.Status = TransactionStatus.Completed;
            transaction.UpdatedAt = DateTime.UtcNow;

            var planner = await _context.Users.FindAsync(transaction.UserId);
            if (planner == null || planner.Role != UserRole.Planner)
                throw new InvalidOperationException("Planner not found or role mismatch.");

            planner.Balance += transaction.Amount;

            await _context.SaveChangesAsync();

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
                UpdatedAt = transaction.UpdatedAt
            };
        }

        public async Task<decimal> GetPlannerBalanceAsync(int plannerId)
        {
            var planner = await _context.Users.FindAsync(plannerId);
            if (planner == null || planner.Role != UserRole.Planner)
                throw new ArgumentException("Planner not found.");

            return planner.Balance;
        }
    }
}