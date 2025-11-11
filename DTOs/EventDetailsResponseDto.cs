namespace EventManager.Api.Dtos
{
    public class EventDetailsResponseDto
    {
        public EventResponseDto? Event { get; set; }
        public List<GuestResponseDto>? Guests { get; set; }
        public List<QrCodeResponseDto>? QrCodes { get; set; }
    }
}