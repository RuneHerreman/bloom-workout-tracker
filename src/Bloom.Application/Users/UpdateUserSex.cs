using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

/// <param name="Sex">"Male", "Female", or null to clear it.</param>
public sealed record UpdateUserSexInput(string? Sex);

public sealed record UpdateUserSexOutput;

public class UpdateUserSex(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateUserSex> logger
) : IUseCase<UpdateUserSexInput, UpdateUserSexOutput>
{
    public async Task<UpdateUserSexOutput> Execute(UpdateUserSexInput input, CancellationToken ct = default)
    {
        Sex? sex = null;
        if (!string.IsNullOrWhiteSpace(input.Sex))
        {
            // Only the names: Enum.TryParse would also accept "0" or "1".
            if (!Enum.GetNames<Sex>().Contains(input.Sex.Trim(), StringComparer.OrdinalIgnoreCase))
                throw new BloomGeneralException("Sex must be Male, Female or empty.");
            sex = Enum.Parse<Sex>(input.Sex.Trim(), ignoreCase: true);
        }

        var userId = currentUser.UserId;
        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);
        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        user.Value.SetSex(sex);
        await userRepo.Save(user.Value);
        await uow.Do(ct);

        logger.LogInformation("Sex updated | Id: {UserId}", userId);
        return new UpdateUserSexOutput();
    }
}
