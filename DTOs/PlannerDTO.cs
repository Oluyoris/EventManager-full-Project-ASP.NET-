namespace EventManager.Api.Dtos
{
    public class PlannerDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCode { get; set; }
        public string? Country { get; set; }
        public string? CompanyName { get; set; }
        public string? Role { get; set; }
        public bool IsBlocked { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}