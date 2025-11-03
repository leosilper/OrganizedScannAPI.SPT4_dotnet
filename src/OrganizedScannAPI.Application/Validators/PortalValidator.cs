using FluentValidation;
using OrganizedScannApi.Domain.Entities;

namespace OrganizedScannApi.Application.Validators
{
    public class PortalValidator : AbstractValidator<Portal>
    {
        public PortalValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        }
    }
}
