using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record LoginUserInput(
    string Email,
    string Password
);

public sealed class LoginUser(
    IUnitOfWork uow,
    ILogger<LoginUser> logger
): IUseCase<LoginUserInput, LoginUserOutput>
{
    public async Task<LoginUserOutput> Execute(LoginUserInput input)
    {
        var userRepository = uow.Repo<IUserRepository>();
        var existingUser = await userRepository.GetUserByEmail(input.Email);

        if (existingUser is  null)
            throw new UserDoesNotExistError($"User with email {input.Email} does not exist.");

        return new LoginUserOutput(existingUser.Id.Value.ToString(), existingUser.Email);
    }
}

public sealed record LoginUserOutput(string UserId, string Email);
