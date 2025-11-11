namespace EventManager.Api.Dtos
{
    public class UserRegisterDto
    {
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCode { get; set; }
        public string? Country { get; set; }
        public string? CompanyName { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Role { get; set; } // Added Role property
    }
}