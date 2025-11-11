namespace EventManager.Api.Dtos
{
    public class DashboardAnalyticsDto
    {
        public int TotalEvents { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalIncome { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalUpcomingEvents { get; set; }
        public int TotalCompletedEvents { get; set; }
    }
}