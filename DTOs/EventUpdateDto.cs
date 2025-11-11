using System;

namespace EventManager.Api.Dtos
{
    public class EventUpdateDto
    {
        public string? EventName { get; set; }
        public string? Location { get; set; }
        public DateTime Date { get; set; }
        public string? Time { get; set; }
        public string? Description { get; set; }
        public int NumberOfGuests { get; set; }
    }
}