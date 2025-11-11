using EventManager.Api.Data;
using EventManager.Api.Dtos;
using EventManager.Api.Models;
using EventManager.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EventManager.Api.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly EventManagerDbContext _context;

        public SettingsService(EventManagerDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentMethodDto> UpdatePaymentMethodAsync(int adminId, PaymentMethodUpdateDto paymentMethodDto)
        {
            var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync();
            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod();
                _context.PaymentMethods.Add(paymentMethod);
            }

            paymentMethod.BankName = paymentMethodDto.BankName ?? paymentMethod.BankName;
            paymentMethod.AccountName = paymentMethodDto.AccountName ?? paymentMethod.AccountName;
            paymentMethod.AccountNumber = paymentMethodDto.AccountNumber ?? paymentMethod.AccountNumber;
            paymentMethod.IsManualActive = paymentMethodDto.IsManualActive;
            paymentMethod.PaystackPublicKey = paymentMethodDto.PaystackPublicKey ?? paymentMethod.PaystackPublicKey;
            paymentMethod.PaystackSecretKey = paymentMethodDto.PaystackSecretKey ?? paymentMethod.PaystackSecretKey;
            paymentMethod.IsPaystackActive = paymentMethodDto.IsPaystackActive;
            paymentMethod.FlutterwavePublicKey = paymentMethodDto.FlutterwavePublicKey ?? paymentMethod.FlutterwavePublicKey;
            paymentMethod.FlutterwaveSecretKey = paymentMethodDto.FlutterwaveSecretKey ?? paymentMethod.FlutterwaveSecretKey;
            paymentMethod.IsFlutterwaveActive = paymentMethodDto.IsFlutterwaveActive;
            paymentMethod.PaypalClientId = paymentMethodDto.PaypalClientId ?? paymentMethod.PaypalClientId;
            paymentMethod.PaypalClientSecret = paymentMethodDto.PaypalClientSecret ?? paymentMethod.PaypalClientSecret;
            paymentMethod.IsPaypalActive = paymentMethodDto.IsPaypalActive;
            paymentMethod.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PaymentMethodDto
            {
                BankName = paymentMethod.BankName,
                AccountName = paymentMethod.AccountName,
                AccountNumber = paymentMethod.AccountNumber,
                IsManualActive = paymentMethod.IsManualActive,
                PaystackPublicKey = paymentMethod.PaystackPublicKey,
                PaystackSecretKey = paymentMethod.PaystackSecretKey,
                IsPaystackActive = paymentMethod.IsPaystackActive,
                FlutterwavePublicKey = paymentMethod.FlutterwavePublicKey,
                FlutterwaveSecretKey = paymentMethod.FlutterwaveSecretKey,
                IsFlutterwaveActive = paymentMethod.IsFlutterwaveActive,
                PaypalClientId = paymentMethod.PaypalClientId,
                PaypalClientSecret = paymentMethod.PaypalClientSecret,
                IsPaypalActive = paymentMethod.IsPaypalActive
            };
        }

        public async Task<SiteSettingsDto> UpdateSiteSettingsAsync(int adminId, SiteSettingsUpdateDto settingsDto)
        {
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            if (siteSettings == null)
            {
                siteSettings = new SiteSettings();
                _context.SiteSettings.Add(siteSettings);
            }

            siteSettings.SiteName = settingsDto.SiteName ?? siteSettings.SiteName;
            siteSettings.EventPricePerGuest = settingsDto.EventPricePerGuest > 0 ? settingsDto.EventPricePerGuest : siteSettings.EventPricePerGuest;
            siteSettings.GuestDetailsFee = settingsDto.GuestDetailsFee ?? siteSettings.GuestDetailsFee;
            siteSettings.Currency = settingsDto.Currency ?? siteSettings.Currency;
            siteSettings.EmailProvider = settingsDto.EmailProvider ?? siteSettings.EmailProvider;
            siteSettings.SmtpHost = settingsDto.SmtpHost ?? siteSettings.SmtpHost;
            siteSettings.SmtpPort = settingsDto.SmtpPort > 0 ? settingsDto.SmtpPort : siteSettings.SmtpPort;
            siteSettings.SmtpUsername = settingsDto.SmtpUsername ?? siteSettings.SmtpUsername;
            siteSettings.SmtpPassword = settingsDto.SmtpPassword ?? siteSettings.SmtpPassword;
            siteSettings.SmtpUseSsl = settingsDto.SmtpUseSsl;
            siteSettings.SendGridApiKey = settingsDto.SendGridApiKey ?? siteSettings.SendGridApiKey;
            siteSettings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new SiteSettingsDto
            {
                SiteName = siteSettings.SiteName,
                EventPricePerGuest = siteSettings.EventPricePerGuest,
                GuestDetailsFee = siteSettings.GuestDetailsFee,
                Currency = siteSettings.Currency,
                EmailProvider = siteSettings.EmailProvider,
                SmtpHost = siteSettings.SmtpHost,
                SmtpPort = siteSettings.SmtpPort,
                SmtpUsername = siteSettings.SmtpUsername,
                SmtpPassword = siteSettings.SmtpPassword,
                SmtpUseSsl = siteSettings.SmtpUseSsl,
                SendGridApiKey = siteSettings.SendGridApiKey
            };
        }

        public async Task<PaymentMethodDto> GetPaymentMethodAsync()
        {
            var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync();
            if (paymentMethod == null)
            {
                throw new ArgumentException("Payment method not configured.");
            }

            return new PaymentMethodDto
            {
                BankName = paymentMethod.BankName,
                AccountName = paymentMethod.AccountName,
                AccountNumber = paymentMethod.AccountNumber,
                IsManualActive = paymentMethod.IsManualActive,
                PaystackPublicKey = paymentMethod.PaystackPublicKey,
                PaystackSecretKey = paymentMethod.PaystackSecretKey,
                IsPaystackActive = paymentMethod.IsPaystackActive,
                FlutterwavePublicKey = paymentMethod.FlutterwavePublicKey,
                FlutterwaveSecretKey = paymentMethod.FlutterwaveSecretKey,
                IsFlutterwaveActive = paymentMethod.IsFlutterwaveActive,
                PaypalClientId = paymentMethod.PaypalClientId,
                PaypalClientSecret = paymentMethod.PaypalClientSecret,
                IsPaypalActive = paymentMethod.IsPaypalActive
            };
        }

        public async Task<SiteSettingsDto?> GetSiteSettingsAsync()
        {
            var siteSettings = await _context.SiteSettings.FirstOrDefaultAsync();
            if (siteSettings == null)
            {
                Console.WriteLine("SiteSettings not found in SettingsService.GetSiteSettingsAsync.");
                return null;
            }

            Console.WriteLine($"SiteSettings found in SettingsService: {siteSettings.SiteName}, {siteSettings.EventPricePerGuest}");
            return new SiteSettingsDto
            {
                SiteName = siteSettings.SiteName,
                EventPricePerGuest = siteSettings.EventPricePerGuest,
                GuestDetailsFee = siteSettings.GuestDetailsFee,
                Currency = siteSettings.Currency,
                EmailProvider = siteSettings.EmailProvider,
                SmtpHost = siteSettings.SmtpHost,
                SmtpPort = siteSettings.SmtpPort,
                SmtpUsername = siteSettings.SmtpUsername,
                SmtpPassword = siteSettings.SmtpPassword,
                SmtpUseSsl = siteSettings.SmtpUseSsl,
                SendGridApiKey = siteSettings.SendGridApiKey
            };
        }
    }
}