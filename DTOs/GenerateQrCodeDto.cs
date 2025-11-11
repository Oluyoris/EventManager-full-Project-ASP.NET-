namespace EventManager.Api.Dtos
{
    public class GenerateQrCodeDto
    {
        public int EventId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
    }
}