using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record RenewSessionInput(UserId UserId, string Stamp, bool Persistent);

public sealed record RenewSessionOutput(string Token);

/// <summary>
/// Re-issues a session token for an active user. Refuses (and so ends the
/// session) when the user is gone or their password changed since the token
/// was issued.
/// </summary>
public class RenewSession(
    IUnitOfWork uow,
    ITokenIssuer tokenIssuer,
    ILogger<RenewSession> logger
) : IUseCase<RenewSessionInput, RenewSessionOutput>
{
    public async Task<RenewSessionOutput> Execute(RenewSessionInput input, CancellationToken ct = default)
    {
        var user = await uow.Repo<IUserRepository>().ById(input.UserId);

        if (!user.HasValue)
            throw new UnauthenticatedException("Session user no longer exists.");

        if (SessionTicket.StampOf(user.Value) != input.Stamp)
        {
            logger.LogInformation("Session renewal refused, password changed | UserId: {UserId}", input.UserId);
            throw new UnauthenticatedException("Session was ended by a password change.");
        }

        return new RenewSessionOutput(tokenIssuer.Issue(SessionTicket.For(user.Value, input.Persistent)));
    }
}
