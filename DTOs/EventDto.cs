using System;

namespace EventManager.Api.Dtos
{
    public class EventDto
    {
        public string? Id { get; set; } // Changed to string to match e.Id.ToString()
        public string? EventName { get; set; }
        public string? Location { get; set; }
        public string? Date { get; set; } // Already a string in the mapping
        public string? Time { get; set; }
        public string? Description { get; set; }
        public int NumberOfGuests { get; set; }
        public string? Status { get; set; } // Changed to string to match e.Status.ToString()
        public DateTime CreatedAt { get; set; } // Changed to DateTime to match e.CreatedAt
        public DateTime? UpdatedAt { get; set; } // Changed to DateTime? to match e.UpdatedAt
    }
}