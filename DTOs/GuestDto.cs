namespace EventManager.Api.Dtos
{
    public class GuestDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? QrCode { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}