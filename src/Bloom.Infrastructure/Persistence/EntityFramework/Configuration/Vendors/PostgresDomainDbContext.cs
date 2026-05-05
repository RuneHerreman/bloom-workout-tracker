using Bloom.Infrastructure.Persistence.EntityFramework.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Vendors;

public sealed class PostgresDomainDbContext(
    IServiceProvider serviceProvider,
    IConfiguration configuration
) : DomainDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        PublishDomainEventsInterceptor publishDomainEventsInterceptor = serviceProvider.GetService<PublishDomainEventsInterceptor>()!;
        
        if (publishDomainEventsInterceptor is not null)
            optionsBuilder.AddInterceptors(publishDomainEventsInterceptor);

        optionsBuilder.UseNpgsql(BuildConnectionString());
    }

    private string BuildConnectionString()
    {
        string connectionString = configuration.GetValue<string>("DefaultConnection:ConnectionString")!;
        string username = configuration.GetValue<string>("DefaultConnection:Username")!;
        string password = configuration.GetValue<string>("DefaultConnection:Password")!;

        return connectionString.Replace("{username}", username).Replace("{password}", password);
    }
}