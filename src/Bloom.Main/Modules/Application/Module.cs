using Bloom.Application.Contracts.Ports;

namespace Bloom.Main.Modules.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // --------------------------------------------------------------------
        // -------------------- Register command use cases --------------------
        // --------------------------------------------------------------------
        
        // ------------------------------------------------------------------
        // -------------------- Register query use cases --------------------
        // ------------------------------------------------------------------
        
        return services;
    }
}