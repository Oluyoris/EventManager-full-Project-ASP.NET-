using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class QrCodeValidator : AbstractValidator<ScanQrCodeDto>
    {
        public QrCodeValidator()
        {
            RuleFor(x => x.QrCodeValue)
                .NotEmpty().WithMessage("QR code value is required.")
                .MaximumLength(50).WithMessage("QR code value must not exceed 50 characters.");

            RuleFor(x => x.EventId)
                .GreaterThan(0).WithMessage("Event ID must be greater than 0.");
        }
    }
}