using EventManager.Api.Dtos;
using EventManager.Api.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventManager.Api.Services.Interfaces
{
    public interface IQrCodeService
    {
        Task<QrCode> GenerateQrCodeAsync(GenerateQrCodeDto qrCodeDto);
        Task<Guest> ScanQrCodeAsync(ScanQrCodeDto scanDto, ClaimsPrincipal? user = null);
        Task<QrCode> GetQrCodeByIdAsync(int qrCodeId);
    }
}