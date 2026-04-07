using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public static class LoginUserController
{
    public static async Task<Results<Ok, UnauthorizedHttpResult>> Invoke(
        [FromBody] string command
    )
    {
        throw new NotImplementedException();
    }
}