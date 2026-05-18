using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Strava;

namespace Bloom.Application.Strava;

public sealed record ConnectStravaInput(
    long StravaAthleteId,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string AthleteName
);

public sealed record ConnectStravaOutput(string AthleteName);

public class ConnectStrava(IUnitOfWork uow, ICurrentUser currentUser) : IUseCase<ConnectStravaInput, ConnectStravaOutput>
{
    public async Task<ConnectStravaOutput> Execute(ConnectStravaInput input, CancellationToken ct = default)
    {
        var repo = uow.Repo<IStravaConnectionRepository>();
        var existing = await repo.ByUserId(currentUser.UserId, ct);

        if (existing.HasValue)
        {
            existing.Value.UpdateTokens(input.AccessToken, input.RefreshToken, input.ExpiresAt);
            await repo.Save(existing.Value);
        }
        else
        {
            var connection = StravaConnection.Create(
                currentUser.UserId,
                input.StravaAthleteId,
                input.AccessToken,
                input.RefreshToken,
                input.ExpiresAt,
                input.AthleteName
            );
            await repo.Save(connection);
        }

        await uow.Do(ct);
        return new ConnectStravaOutput(input.AthleteName);
    }
}
