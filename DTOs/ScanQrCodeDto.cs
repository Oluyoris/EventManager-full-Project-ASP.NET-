namespace EventManager.Api.Dtos
{
    public class ScanQrCodeDto
    {
        public int EventId { get; set; }
        public required string? QrCodeValue { get; set; }
    }
}