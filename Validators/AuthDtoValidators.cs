using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
    {
        public UserRegisterDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[1-9]\d{1,14}$");
            RuleFor(x => x.CountryCode).NotEmpty().Matches(@"^\+\d{1,4}$");
            RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least one special character.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }

    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.UsernameOrEmail).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}