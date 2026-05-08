using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Auth;

namespace Bloom.Main.Modules.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}
