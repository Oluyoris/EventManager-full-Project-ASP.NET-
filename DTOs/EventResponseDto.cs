using EventManager.Api.Models;
using System;
using System.Text.Json.Serialization;

namespace EventManager.Api.Dtos
{
    public class EventResponseDto
    {
        public int Id { get; set; }
        public string? EventName { get; set; }
        public string? Location { get; set; }
        public DateTime Date { get; set; }
        public string? Time { get; set; }
        public string? Description { get; set; }
        public int NumberOfGuests { get; set; }
        public int PlannerId { get; set; }
        public string? PlannerName { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EventStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}