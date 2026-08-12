using FluentValidation;

namespace Nearbly.Application.Features.Tabs;

public sealed class CreateTabRequestValidator : AbstractValidator<CreateTabRequest>
{
    public CreateTabRequestValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateTabRequestValidator : AbstractValidator<UpdateTabRequest>
{
    public UpdateTabRequestValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
