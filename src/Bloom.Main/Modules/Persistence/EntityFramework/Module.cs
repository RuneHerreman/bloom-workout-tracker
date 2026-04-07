namespace Bloom.Main.Modules.Persistence.EntityFramework;

public static class Module
{
    public static IServiceCollection AddEFCoreModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services.AddEFCoreServices(configuration);
    }
}