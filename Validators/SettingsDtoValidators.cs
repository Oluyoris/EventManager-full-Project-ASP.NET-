using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class PaymentMethodUpdateDtoValidator : AbstractValidator<PaymentMethodUpdateDto>
    {
        public PaymentMethodUpdateDtoValidator()
        {
            RuleFor(x => x.BankName).NotEmpty().When(x => x.IsManualActive);
            RuleFor(x => x.AccountName).NotEmpty().When(x => x.IsManualActive);
            RuleFor(x => x.AccountNumber).NotEmpty().When(x => x.IsManualActive);
            RuleFor(x => x.PaystackPublicKey).NotEmpty().When(x => x.IsPaystackActive);
            RuleFor(x => x.PaystackSecretKey).NotEmpty().When(x => x.IsPaystackActive);
            RuleFor(x => x.FlutterwavePublicKey).NotEmpty().When(x => x.IsFlutterwaveActive);
            RuleFor(x => x.FlutterwaveSecretKey).NotEmpty().When(x => x.IsFlutterwaveActive);
            RuleFor(x => x.PaypalClientId).NotEmpty().When(x => x.IsPaypalActive);
            RuleFor(x => x.PaypalClientSecret).NotEmpty().When(x => x.IsPaypalActive);
        }
    }

    public class SiteSettingsUpdateDtoValidator : AbstractValidator<SiteSettingsUpdateDto>
    {
        public SiteSettingsUpdateDtoValidator()
        {
            RuleFor(x => x.SiteName).NotEmpty();
            RuleFor(x => x.EventPricePerGuest).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty();
            RuleFor(x => x.EmailProvider).NotEmpty().Must(x => new[] { "SMTP", "SendGrid" }.Contains(x)).WithMessage("Email provider must be 'SMTP' or 'SendGrid'.");
            RuleFor(x => x.SmtpHost).NotEmpty().When(x => x.EmailProvider == "SMTP");
            RuleFor(x => x.SmtpPort).GreaterThan(0).When(x => x.EmailProvider == "SMTP");
            RuleFor(x => x.SmtpUsername).NotEmpty().When(x => x.EmailProvider == "SMTP");
            RuleFor(x => x.SmtpPassword).NotEmpty().When(x => x.EmailProvider == "SMTP");
            RuleFor(x => x.SendGridApiKey).NotEmpty().When(x => x.EmailProvider == "SendGrid");
        }
    }
}