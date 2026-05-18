using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Strava;

namespace Bloom.Main.Modules.Strava;

public static class Module
{
    public static IServiceCollection AddStravaModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<StravaOptions>(configuration.GetSection(StravaOptions.SectionName));

        services.AddHttpClient<StravaApiClient>();
        services.AddScoped<StravaActivityMapper>();
        services.AddScoped<IStravaActivityImporter, StravaActivityImporter>();

        return services;
    }
}
