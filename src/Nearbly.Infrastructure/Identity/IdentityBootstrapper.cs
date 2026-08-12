using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nearbly.Infrastructure.Persistence;

namespace Nearbly.Infrastructure.Identity;

public sealed class IdentityBootstrapper(UserManager<IdentityUser> userManager, IOptions<BootstrapAdminOptions> options, ILogger<IdentityBootstrapper> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.Password))
        {
            logger.LogInformation("Bootstrap admin is not configured; skipping account creation.");
            return;
        }

        var email = settings.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(email) is not null)
            return;
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, settings.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Could not create bootstrap admin: {string.Join("; ", result.Errors.Select(error => error.Code))}");
        logger.LogInformation("Bootstrap admin account created for {Email}.", email);
    }
}
