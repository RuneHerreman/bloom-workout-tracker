using Microsoft.AspNetCore.Http;

namespace Bloom.Infrastructure.Auth;

public static class AuthCookie
{
    /// <summary>
    /// Writes the session token. A "Remember me" session gets a persistent
    /// cookie; otherwise it is a browser-session cookie and the token's own
    /// expiry is the idle timeout.
    /// </summary>
    public static void Append(HttpContext httpContext, JwtOptions options, string token, bool persistent, DateTimeOffset now)
    {
        httpContext.Response.Cookies.Append(options.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps,
            Expires = persistent ? now + options.LifetimeFor(persistent: true) : null,
            Path = "/"
        });
    }

    public static void Delete(HttpContext httpContext, JwtOptions options)
    {
        httpContext.Response.Cookies.Delete(options.CookieName, new CookieOptions
        {
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps
        });
    }
}
