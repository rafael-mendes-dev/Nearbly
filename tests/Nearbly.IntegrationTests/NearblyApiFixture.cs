using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace Nearbly.IntegrationTests;

public sealed class NearblyApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("nearbly_test")
        .WithUsername("nearbly")
        .WithPassword("nearbly")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", container.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "integration-test-signing-key-with-at-least-32-bytes");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Email", "admin@test.local");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Password", "ChangeMe123");
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = container.GetConnectionString(),
                ["Jwt:SigningKey"] = "integration-test-signing-key-with-at-least-32-bytes",
                ["BootstrapAdmin:Email"] = "admin@test.local",
                ["BootstrapAdmin:Password"] = "ChangeMe123"
            }));
        });
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", null);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
        Environment.SetEnvironmentVariable("BootstrapAdmin__Email", null);
        Environment.SetEnvironmentVariable("BootstrapAdmin__Password", null);
        await container.DisposeAsync();
    }
}
