using EventManager.Api.Data;
using EventManager.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EventManager.Api
{
    public static class SeedData
    {
        public static async Task Initialize(EventManagerDbContext context)
        {
            Console.WriteLine("Starting database seeding...");

            // Ensure the database exists, but don't delete it
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Database ensured created.");

            // Seed SiteSettings if none exist
            if (!await context.SiteSettings.AnyAsync())
            {
                Console.WriteLine("Seeding SiteSettings...");
                context.SiteSettings.Add(new SiteSettings
                {
                    Id = 1,
                    SiteName = "EventManager",
                    EventPricePerGuest = 500,
                    GuestDetailsFee = 100,
                    Currency = "NGN",
                    EmailProvider = "SMTP",
                    SmtpHost = "smtp.example.com",
                    SmtpPort = 587,
                    SmtpUsername = "example@eventmanager.com",
                    SmtpPassword = "examplepassword",
                    SmtpUseSsl = true,
                    SendGridApiKey = null,
                    CreatedAt = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
            else
            {
                Console.WriteLine("SiteSettings table already has data, skipping seeding.");
            }

            // Seed PaymentMethods if none exist
            if (!await context.PaymentMethods.AnyAsync())
            {
                Console.WriteLine("Seeding PaymentMethods...");
                context.PaymentMethods.Add(new PaymentMethod
                {
                    Id = 1,
                    BankName = "First Bank",
                    AccountName = "Event Manager Ltd",
                    AccountNumber = "1234567890",
                    IsManualActive = true,
                    IsPaystackActive = false,
                    IsFlutterwaveActive = false,
                    IsPaypalActive = false,
                    CreatedAt = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
            else
            {
                Console.WriteLine("PaymentMethods table already has data, skipping seeding.");
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Database seeding completed.");
        }
    }
}