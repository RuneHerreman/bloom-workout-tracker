using Bloom.Main.Modules.Application;
using Bloom.Main.Modules.Auth;
using Bloom.Main.Modules.Persistence.EntityFramework;
using Bloom.Main.Modules.Strava;
using Bloom.Main.Modules.WebApi;

namespace Bloom.Main.Modules;

public static class Modules
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddMemoryCache()
            .AddApplicationModule(configuration)
            .AddAuthModule(configuration)
            .AddEFCoreModule(configuration)
            .AddStravaModule(configuration)
            .AddWebApiModule(configuration);
    }

    public static async Task<WebApplication> UseModules(this WebApplication app)
    {
        return await app.UseWebApiModule();
    }
}