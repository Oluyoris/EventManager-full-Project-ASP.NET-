using System;

namespace EventManager.Api.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public decimal EventPricePerGuest { get; set; }
        public decimal GuestDetailsFee { get; set; }
        public string? Currency { get; set; } = string.Empty;
        public string? EmailProvider { get; set; } = string.Empty;
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string? SendGridApiKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}