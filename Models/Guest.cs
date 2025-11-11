using System;

namespace EventManager.Api.Models
{
    public enum GuestStatus
    {
        Pending,
        Present,
        Absent
    }

    public class Guest
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? QrCode { get; set; } = string.Empty;
        public GuestStatus Status { get; set; } = GuestStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}