using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100).When(x => x.FullName != null);
            RuleFor(x => x.PhoneNumber).Matches(@"^\+?[1-9]\d{1,14}$").When(x => x.PhoneNumber != null);
            RuleFor(x => x.CountryCode).Matches(@"^\+\d{1,4}$").When(x => x.CountryCode != null);
            RuleFor(x => x.Country).MaximumLength(100).When(x => x.Country != null);
            RuleFor(x => x.CompanyName).MaximumLength(100).When(x => x.CompanyName != null);
        }
    }

    public class PasswordUpdateDtoValidator : AbstractValidator<PasswordUpdateDto>
    {
        public PasswordUpdateDtoValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
                .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("New password must contain at least one number.")
                .Matches(@"[!@#$%^&*]").WithMessage("New password must contain at least one special character.");
            RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage("New passwords do not match.");
        }
    }

    public class PlannerDtoValidator : AbstractValidator<PlannerDto>
    {
        public PlannerDtoValidator()
        {
            // Response-only DTO, minimal validation
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}