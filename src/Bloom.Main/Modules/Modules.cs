using Bloom.Main.Modules.Application;
using Bloom.Main.Modules.Persistence.EntityFramework;
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
            .AddApplicationModule(configuration)
            .AddEFCoreModule(configuration)
            .AddWebApiModule(configuration);
    }

    public static async Task<WebApplication> UseModules(this WebApplication app)
    {
        return await app.UseWebApiModule();
    }
}