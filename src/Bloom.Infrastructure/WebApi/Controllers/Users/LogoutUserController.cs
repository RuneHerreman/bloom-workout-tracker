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
        AuthCookie.Delete(httpContext, jwtOptions.Value);

        return TypedResults.NoContent();
    }
}
