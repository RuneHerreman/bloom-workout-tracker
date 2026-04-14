using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public record LoginRequest(
    string Email,
    string Password
);
public record LoginResponse(string Token);

public static class LoginUserController
{
    public static async Task<Results<Ok<LoginResponse>, BadRequest<string>>> Invoke(
        [FromBody] LoginRequest input,
        [FromServices] IUseCase<LoginUserInput, LoginUserOutput> loginUser
    )
    {
        try
        {
            var result = await loginUser.Execute(new LoginUserInput(
                input.Email,
                input.Password
            ));
            
            var token = JwtProvider.GenerateToken(result.UserId, result.Email);
            
            return TypedResults.Ok(new LoginResponse(token));
        }
        catch (Exception e)
        {
            return TypedResults.BadRequest(e.Message);
        }
    }
}