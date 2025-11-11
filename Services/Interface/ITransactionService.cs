using EventManager.Api.Dtos;
using EventManager.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionDto> ApprovePaymentAsync(int adminId, ApprovePaymentDto approveDto);
        Task<List<TransactionDto>> GetTransactionsByPlannerAsync(int plannerId);
        Task<List<TransactionDto>> GetAllTransactionsAsync();
        Task<List<TransactionDto>> GetPendingTransactionsAsync();
        Task<List<TransactionDto>> GetApprovedTransactionsAsync();
        Task<List<TransactionDto>> GetRejectedTransactionsAsync();
    }
}