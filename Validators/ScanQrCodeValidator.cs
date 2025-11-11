using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class ScanQrCodeValidator : AbstractValidator<ScanQrCodeDto>
    {
        public ScanQrCodeValidator()
        {
            RuleFor(dto => dto.EventId)
                .GreaterThan(0)
                .WithMessage("Event ID must be greater than 0.");

            RuleFor(dto => dto.QrCodeValue)
                .NotEmpty()
                .WithMessage("QR code value is required.")
                .MaximumLength(50)
                .WithMessage("QR code value cannot exceed 50 characters.");
        }
    }
}