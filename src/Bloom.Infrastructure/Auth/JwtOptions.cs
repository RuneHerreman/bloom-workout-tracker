namespace Bloom.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;

    /// <summary>Idle timeout of a normal session: a token lives this long after its last renewal.</summary>
    public int ExpiryMinutes { get; init; } = 60;

    /// <summary>Idle timeout of a "Remember me" session; its cookie also survives a browser restart.</summary>
    public int RememberMeDays { get; init; } = 30;

    public string CookieName { get; init; } = "auth_token";

    /// <summary>
    /// A token older than this is re-issued on the next authenticated request
    /// (and the user re-checked), so an active user never reaches expiry.
    /// </summary>
    public TimeSpan RenewAfter => TimeSpan.FromMinutes(ExpiryMinutes / 2.0);

    public TimeSpan LifetimeFor(bool persistent) =>
        persistent ? TimeSpan.FromDays(RememberMeDays) : TimeSpan.FromMinutes(ExpiryMinutes);
}
