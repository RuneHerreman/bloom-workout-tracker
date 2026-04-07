using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public record RegisterResponse(string Token);

public class RegisterUserController
{
    public static async Task<Results<Created<RegisterResponse>, BadRequest>> Invoke(
        [FromBody] RegisterUserInput input,
        [FromServices] IUseCase<RegisterUserInput, RegisterUserOutput> registerUser,
        IConfiguration configuration)
    {
        var result = await registerUser.Execute(input);
        
        var token = JwtProvider.GenerateToken(result.UserId, result.Email);
        
        return TypedResults.Created($"/api/users/{result.UserId}", new RegisterResponse(token));
    }
}