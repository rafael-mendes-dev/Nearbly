using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nearbly.Infrastructure.Persistence;

public sealed class NearblyDbContextFactory : IDesignTimeDbContextFactory<NearblyDbContext>
{
    public NearblyDbContext CreateDbContext(string[] args)
    {
        var connectionString = NpgsqlConnectionStringNormalizer.Normalize(
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=nearbly;Username=nearbly;Password=nearbly");
        var options = new DbContextOptionsBuilder<NearblyDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(NearblyDbContext).Assembly.FullName);
                npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            })
            .UseSnakeCaseNamingConvention()
            .Options;
        return new NearblyDbContext(options);
    }
}
