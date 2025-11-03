using FluentValidation;
using OrganizedScannApi.Domain.Entities;

namespace OrganizedScannApi.Application.Validators
{
    public class MotorcycleValidator : AbstractValidator<Motorcycle>
    {
        public MotorcycleValidator()
        {
            RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Rfid).NotEmpty().MaximumLength(64);
            RuleFor(x => x.PortalId).GreaterThan(0);
            RuleFor(x => x.EntryDate).LessThanOrEqualTo(x => x.AvailabilityForecast);
        }
    }
}
