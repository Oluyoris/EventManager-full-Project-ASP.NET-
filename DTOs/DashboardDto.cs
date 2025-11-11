namespace EventManager.Api.Dtos
{
    public class DashboardDto
    {
        public string? PlannerName { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public decimal WalletBalance { get; set; }
        public List<EventDto> RecentEvents { get; set; } = new List<EventDto>();
    }
} 