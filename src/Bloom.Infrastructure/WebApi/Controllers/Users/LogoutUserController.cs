using Bloom.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public static class LogoutUserController
{
    public static NoContent Invoke(
        HttpContext httpContext,
        IOptions<JwtOptions> jwtOptions
    )
    {
        httpContext.Response.Cookies.Delete(jwtOptions.Value.CookieName, new CookieOptions
        {
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps
        });

        return TypedResults.NoContent();
    }
}
