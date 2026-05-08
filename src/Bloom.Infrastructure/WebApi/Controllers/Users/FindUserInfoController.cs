using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record FindUserInfoRequest(
    [FromRoute] Guid UserId,
    [FromServices] IUseCase<FindUserInfoInput, FindUserInfoOutput> UseCase
);

public static class FindUserInfoController
{
    public static async Task<Results<Ok<User>, BadRequest>> Invoke(
        [AsParameters] FindUserInfoRequest request
    )
    {
        var output = await request.UseCase.Execute(new FindUserInfoInput(request.UserId));

        return TypedResults.Ok(output.User.ToResponse());
    }
}
