using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Vendors;

public sealed class PostgresQueryDbContext(
    IConfiguration configuration
) : QueryDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(BuildConnectionString());
    }

    private string BuildConnectionString()
    {
        string connectionString = configuration.GetValue<string>("Database:ConnectionString")!;
        string username = configuration.GetValue<string>("Database:Username")!;
        string password = configuration.GetValue<string>("Database:Password")!;

        return connectionString.Replace("{username}", username).Replace("{password}", password);
    }
}