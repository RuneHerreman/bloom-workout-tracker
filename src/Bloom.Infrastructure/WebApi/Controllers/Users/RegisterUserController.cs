using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public record RegisterRequest(
    string Email,
    string Password,
    string Name,
    decimal Height,
    decimal Weight
);
public record RegisterResponse(string Token);

public class RegisterUserController
{
    public static async Task<Results<Created<RegisterResponse>, BadRequest>> Invoke(
        [FromBody] RegisterRequest input,
        [FromServices] IUseCase<RegisterUserInput, RegisterUserOutput> registerUser
    )
    {
        var result = await registerUser.Execute(new RegisterUserInput(
            input.Email,
            input.Name,
            input.Password,
            input.Height,
            input.Weight,
            0
        ));
        
        var token = JwtProvider.GenerateToken(result.UserId, result.Email);
        
        return TypedResults.Created($"/api/users/{result.UserId}", new RegisterResponse(token));
    }
}