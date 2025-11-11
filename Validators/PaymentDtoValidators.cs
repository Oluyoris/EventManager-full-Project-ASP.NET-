using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class ManualPaymentDtoValidator : AbstractValidator<ManualPaymentDto>
    {
        public ManualPaymentDtoValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.ProofFileBase64)
                .NotEmpty()
                .WithMessage("Proof file content is required.");

            RuleFor(x => x.ProofFileName)
                .NotEmpty()
                .Must(x => x != null && (x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                                        x.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                        x.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                        x.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Proof file must be a PDF, PNG, JPEG, or JPG.");

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .Must(x => x != null && (x.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
                                        x.Equals("image/png", StringComparison.OrdinalIgnoreCase) ||
                                        x.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Invalid proof file content type. Must be PDF, PNG, or JPEG.");
        }
    }

    public class AutomaticPaymentDtoValidator : AbstractValidator<AutomaticPaymentDto>
    {
        public AutomaticPaymentDtoValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Gateway).NotEmpty().Must(x => new[] { "Paystack", "Flutterwave", "Paypal" }.Contains(x));
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class VerifyPaymentDtoValidator : AbstractValidator<VerifyPaymentDto>
    {
        public VerifyPaymentDtoValidator()
        {
            RuleFor(x => x.Reference).NotEmpty();
            RuleFor(x => x.Gateway).NotEmpty().Must(x => new[] { "Paystack", "Flutterwave", "Paypal" }.Contains(x));
        }
    }

    public class PaymentResponseDtoValidator : AbstractValidator<PaymentResponseDto>
    {
        public PaymentResponseDtoValidator()
        {
            RuleFor(x => x.Reference).NotEmpty();
        }
    }

    public class ApprovePaymentDtoValidator : AbstractValidator<ApprovePaymentDto>
    {
        public ApprovePaymentDtoValidator()
        {
            RuleFor(x => x.TransactionId).GreaterThan(0);
        }
    }
}