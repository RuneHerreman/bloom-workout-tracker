using Bloom.Application.Common.Security;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record RegisterUserInput(
    string Email,
    string Name,
    string Password,
    decimal Height,
    decimal Weight,
    int ActiveDays
);

public sealed class RegisterUser(
    IUnitOfWork uow,
    ILogger<RegisterUser> logger
): IUseCase<RegisterUserInput, RegisterUserOutput>
{
    public async Task<RegisterUserOutput> Execute(RegisterUserInput input)
    {
        var userRepository = uow.Repo<IUserRepository>();
        var existingUser = await userRepository.GetUserByEmail(input.Email);

        if (existingUser is not null)
            throw new UserAlreadyExistsError($"User with email {input.Email} already exists.");
        
        User user = User.Create(
            input.Email,
            input.Name,
            Hashing.Hash(input.Password),
            input.Height,
            input.Weight,
            input.ActiveDays
        );

        await uow.Save<IUserRepository>(user);
        await uow.Do(); 
        
        logger.LogInformation("User registered: {userMail} ({userId})", user.Email, user.Id);
        return new RegisterUserOutput(user.Id.Value.ToString(), user.Email);
    }
}

public sealed record RegisterUserOutput(string UserId, string Email);

