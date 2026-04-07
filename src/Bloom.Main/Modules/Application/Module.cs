using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;

namespace Bloom.Main.Modules.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // -------------------- Register command use cases --------------------
        services.AddScoped<IUseCase<RegisterUserInput, RegisterUserOutput>, RegisterUser>();
        
        
        // -------------------- Register query use cases --------------------
        

        return services;
    }
}