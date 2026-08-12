using FluentValidation;
using Nearbly.Domain.Services;

namespace Nearbly.Application.Features.Stores;

public sealed class CreateStoreRequestValidator : AbstractValidator<CreateStoreRequest>
{
    public CreateStoreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Slug).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(120).Must(IsNormalizedSlugValid).WithMessage("Slug must contain at least one letter or digit after normalization.");
        RuleFor(x => x.LogoUrl).Must(value => value is null || UrlValidator.IsValid(value)).WithMessage("LogoUrl must be an absolute HTTP or HTTPS URL without credentials.");
        RuleFor(x => x.PrimaryColor).Must(ColorValidator.IsValid).WithMessage("PrimaryColor must use #RRGGBB.");
        RuleFor(x => x.SecondaryColor).Must(ColorValidator.IsValid).WithMessage("SecondaryColor must use #RRGGBB.");
    }

    private static bool IsNormalizedSlugValid(string value)
    {
        try { return SlugNormalizer.Normalize(value).Length <= 120; }
        catch (ArgumentException) { return false; }
    }
}

public sealed class UpdateStoreRequestValidator : AbstractValidator<UpdateStoreRequest>
{
    public UpdateStoreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Slug).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(120).Must(IsNormalizedSlugValid).WithMessage("Slug must contain at least one letter or digit after normalization.");
        RuleFor(x => x.LogoUrl).Must(value => value is null || UrlValidator.IsValid(value)).WithMessage("LogoUrl must be an absolute HTTP or HTTPS URL without credentials.");
        RuleFor(x => x.PrimaryColor).Must(ColorValidator.IsValid).WithMessage("PrimaryColor must use #RRGGBB.");
        RuleFor(x => x.SecondaryColor).Must(ColorValidator.IsValid).WithMessage("SecondaryColor must use #RRGGBB.");
    }

    private static bool IsNormalizedSlugValid(string value)
    {
        try { return SlugNormalizer.Normalize(value).Length <= 120; }
        catch (ArgumentException) { return false; }
    }
}
