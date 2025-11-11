namespace EventManager.Api.Dtos
{
    public class SiteSettingsDto
    {
        public string? SiteName { get; set; }
        public decimal EventPricePerGuest { get; set; }
        public decimal? GuestDetailsFee { get; set; }
        public string? Currency { get; set; }
        public string? EmailProvider { get; set; } // "SMTP" or "SendGrid"
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string? SendGridApiKey { get; set; }
    }
}