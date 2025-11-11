using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class EventCreateDtoValidator : AbstractValidator<EventCreateDto>
    {
        public EventCreateDtoValidator()
        {
            RuleFor(x => x.EventName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Date).NotEmpty().GreaterThan(DateTime.UtcNow);
            RuleFor(x => x.Time).NotEmpty().Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$");
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.NumberOfGuests).GreaterThan(0);
        }
    }

    public class EventUpdateDtoValidator : AbstractValidator<EventUpdateDto>
    {
        public EventUpdateDtoValidator()
        {
            RuleFor(x => x.EventName).NotEmpty().MaximumLength(100).When(x => x.EventName != null);
            RuleFor(x => x.Location).MaximumLength(200).When(x => x.Location != null);
            RuleFor(x => x.Date).GreaterThan(DateTime.UtcNow).When(x => x.Date != default);
            RuleFor(x => x.Time).Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$").When(x => x.Time != null);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
            RuleFor(x => x.NumberOfGuests).GreaterThan(0).When(x => x.NumberOfGuests != 0);
        }
    }

    public class EventDtoValidator : AbstractValidator<EventDto>
    {
        public EventDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .Must(id => string.IsNullOrEmpty(id) ? false : int.TryParse(id, out int parsedId) && parsedId > 0)
                .WithMessage("Id must be a positive integer represented as a string.");
        }
    }

    public class GuestDtoValidator : AbstractValidator<GuestDto>
    {
        public GuestDtoValidator()
        {
            RuleFor(x => x.EventId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        }
    }
}