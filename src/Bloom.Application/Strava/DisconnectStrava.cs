using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Strava;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Strava;

public sealed record DisconnectStravaInput;

public class DisconnectStrava(IUnitOfWork uow, ICurrentUser currentUser) : IUseCase<DisconnectStravaInput>
{
    public async Task Execute(DisconnectStravaInput input, CancellationToken ct = default)
    {
        var repo = uow.Repo<IStravaConnectionRepository>();
        var connection = await repo.ByUserId(currentUser.UserId, ct);

        if (!connection.HasValue)
            throw new StravaConnectionNotFoundException("Strava connection not found");

        await repo.Remove(connection.Value);
        await uow.Do(ct);
    }
}
