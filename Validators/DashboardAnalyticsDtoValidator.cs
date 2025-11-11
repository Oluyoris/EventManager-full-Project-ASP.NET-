using EventManager.Api.Dtos;
using FluentValidation;

namespace EventManager.Api.Validators
{
    public class DashboardAnalyticsDtoValidator : AbstractValidator<DashboardAnalyticsDto>
    {
        public DashboardAnalyticsDtoValidator()
        {
            RuleFor(x => x.TotalEvents).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TotalUsers).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TotalIncome).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TotalTransactions).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TotalUpcomingEvents).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TotalCompletedEvents).GreaterThanOrEqualTo(0);
        }
    }
}