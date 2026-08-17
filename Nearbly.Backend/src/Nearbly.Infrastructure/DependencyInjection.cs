using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nearbly.Application.Common;
using Nearbly.Application.Features.Analytics;
using Nearbly.Application.Features.Content;
using Nearbly.Application.Features.Auth;
using Nearbly.Application.Features.Links;
using Nearbly.Application.Features.Public;
using Nearbly.Application.Features.Stores;
using Nearbly.Application.Features.Tabs;
using Nearbly.Application.Features.Media;
using Nearbly.Infrastructure.Identity;
using Nearbly.Infrastructure.Persistence;
using Nearbly.Infrastructure.Storage;

namespace Nearbly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNearblyInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:Default must be configured.");
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<BootstrapAdminOptions>(configuration.GetSection(BootstrapAdminOptions.SectionName));
        services.AddDbContext<NearblyDbContext>(options => options
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(NearblyDbContext).Assembly.FullName);
                npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            })
            .UseSnakeCaseNamingConvention());
        services.AddScoped<INearblyDbContext>(provider => provider.GetRequiredService<NearblyDbContext>());
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
        }).AddEntityFrameworkStores<NearblyDbContext>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes.");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                ValidateAudience = true, ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        services.AddAuthorization();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<CreateStoreRequest>, CreateStoreRequestValidator>();
        services.AddScoped<IValidator<UpdateStoreRequest>, UpdateStoreRequestValidator>();
        services.AddScoped<IValidator<CreateTabRequest>, CreateTabRequestValidator>();
        services.AddScoped<IValidator<UpdateTabRequest>, UpdateTabRequestValidator>();
        services.AddScoped<IValidator<CreateLinkRequest>, CreateLinkRequestValidator>();
        services.AddScoped<IValidator<UpdateLinkRequest>, UpdateLinkRequestValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IdentityBootstrapper>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ITabService, TabService>();
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IPublicService, PublicService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();
        if (string.Equals(configuration["Media:Provider"], "s3", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IObjectStorage, S3ObjectStorage>();
        else
            services.AddSingleton<IObjectStorage, LocalObjectStorage>();
        return services;
    }

    // Schema migrations run via the Railway pre-deploy command (`dotnet Nearbly.Api.dll migrate`,
    // see Program.cs) before a new revision takes traffic, not on every app startup.
    public static async Task InitializeNearblyDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IdentityBootstrapper>().RunAsync();
    }
}
