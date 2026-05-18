using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Strava;

namespace Bloom.Application.Strava;

public sealed record ConnectStravaInput(string Code);
public sealed record ConnectStravaOutput(string AthleteName);

public class ConnectStrava(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IStravaClient stravaClient
) : IUseCase<ConnectStravaInput, ConnectStravaOutput>
{
    public async Task<ConnectStravaOutput> Execute(ConnectStravaInput input, CancellationToken ct = default)
    {
        var token = await stravaClient.ExchangeCode(input.Code, ct);

        var repo = uow.Repo<IStravaConnectionRepository>();
        var existing = await repo.ByUserId(currentUser.UserId, ct);

        if (existing.HasValue)
        {
            existing.Value.UpdateTokens(token.AccessToken, token.RefreshToken, token.ExpiresAt);
            await repo.Save(existing.Value);
        }
        else
        {
            var connection = StravaConnection.Create(
                currentUser.UserId,
                token.AthleteId,
                token.AccessToken,
                token.RefreshToken,
                token.ExpiresAt,
                token.AthleteName
            );
            await repo.Save(connection);
        }

        await uow.Do(ct);
        return new ConnectStravaOutput(token.AthleteName);
    }
}
