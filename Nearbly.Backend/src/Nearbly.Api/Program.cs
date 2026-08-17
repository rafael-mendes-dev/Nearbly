using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Nearbly.Api.Endpoints;
using Nearbly.Api.Infrastructure;
using Nearbly.Infrastructure;
using Nearbly.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddProblemDetails();
builder.Services.AddNearblyInfrastructure(builder.Configuration);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? builder.Configuration["Cors:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? [];
    if (origins.Length > 0) policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Nearbly API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header, Description = "Enter a JWT bearer token." });
    options.OperationFilter<AuthorizationOperationFilter>();
});
builder.Services.AddAuthorization();
builder.Services.AddOutputCache(options => options.AddPolicy("media", policy => policy.Expire(TimeSpan.FromDays(30)).Tag("media")));

var app = builder.Build();

// Invoked as the Railway pre-deploy command (`dotnet Nearbly.Api.dll migrate`) so schema changes
// land before the new revision takes traffic; the runtime image has no `dotnet-ef` CLI, only the
// EF Core provider already referenced by NearblyDbContext, so this calls MigrateAsync directly.
if (args.Contains("migrate", StringComparer.OrdinalIgnoreCase))
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    await migrationScope.ServiceProvider.GetRequiredService<NearblyDbContext>().Database.MigrateAsync();
    return;
}

// The platform's edge proxy (Railway, Cloud Run, etc.) terminates TLS and forwards plain HTTP
// with X-Forwarded-* headers; without this, UseHttpsRedirection below sees the request as HTTP
// and redirect-loops forever.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownIPNetworks = { },
    KnownProxies = { }
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<StatusCodeProblemDetailsMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.UseMiddleware<AdminOperationLoggingMiddleware>();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapPublicEndpoints();
app.MapRedirectEndpoints();

if (!app.Environment.IsEnvironment("Testing"))
    await app.InitializeNearblyDatabaseAsync();

app.Run();

public partial class Program { }
