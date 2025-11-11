using System;

namespace EventManager.Api.Models
{
    public class QrCode
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public string? GuestName { get; set; } = string.Empty;
        public string? QrCodeValue { get; set; } = string.Empty;
        public byte[]? QrCodeImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}