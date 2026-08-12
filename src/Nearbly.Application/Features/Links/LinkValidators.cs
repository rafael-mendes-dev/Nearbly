using FluentValidation;
using Nearbly.Domain.Services;

namespace Nearbly.Application.Features.Links;

public sealed class CreateLinkRequestValidator : AbstractValidator<CreateLinkRequest>
{
    public CreateLinkRequestValidator()
    {
        RuleFor(x => x.Type).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Url).Must(UrlValidator.IsValid).WithMessage("Url must be an absolute HTTP or HTTPS URL without credentials.");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Icon).MaximumLength(120);
    }
}

public sealed class UpdateLinkRequestValidator : AbstractValidator<UpdateLinkRequest>
{
    public UpdateLinkRequestValidator()
    {
        RuleFor(x => x.Type).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Url).Must(UrlValidator.IsValid).WithMessage("Url must be an absolute HTTP or HTTPS URL without credentials.");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Icon).MaximumLength(120);
    }
}
