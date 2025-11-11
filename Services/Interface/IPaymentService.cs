using EventManager.Api.Dtos;
using EventManager.Api.Models;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<TransactionResponseDto> SubmitManualPaymentAsync(int plannerId, ManualPaymentDto paymentDto);
        Task<PaymentResponseDto> InitiateAutomaticPaymentAsync(int plannerId, AutomaticPaymentDto paymentDto);
        Task<TransactionDto> VerifyAutomaticPaymentAsync(VerifyPaymentDto verifyDto);
        Task<decimal> GetPlannerBalanceAsync(int plannerId);
    }
}