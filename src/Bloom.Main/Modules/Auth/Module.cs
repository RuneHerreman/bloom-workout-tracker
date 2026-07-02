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
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer must be configured.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience must be configured.")
            .Validate(o => o.Key.Length >= 32, "Jwt:Key must be at least 32 characters.")
            .Validate(o => !o.Key.StartsWith("__"), "Jwt:Key still holds the appsettings placeholder; set it via the Jwt__Key environment variable.")
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }
}
