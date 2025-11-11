using System;
using System.Collections.Generic;

namespace EventManager.Api.Dtos
{
    public class EventCreateDto
    {
        public string? EventName { get; set; } = string.Empty;
        public string? Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Time { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int NumberOfGuests { get; set; }
        public bool AddGuestDetails { get; set; }
        public List<GuestDto>? Guests { get; set; }
    }
}