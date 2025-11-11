using System;
using System.Collections.Generic;

namespace EventManager.Api.Models
{
    public enum EventStatus
    {
        Draft,
        Pending,
        Active,
        Completed,
        Cancelled
    }

    public class Event
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign key to User
        public string? EventName { get; set; } = string.Empty;
        public string? Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Time { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int NumberOfGuests { get; set; }
        public int PlannerId { get; set; }
        public User? Planner { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<QrCode> QrCodes { get; set; } = new List<QrCode>();
        public List<Guest> Guests { get; set; } = new List<Guest>();
    }
}