using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Strava;

namespace Bloom.Application.Strava;

public sealed record GetStravaStatusInput;
public sealed record GetStravaStatusOutput(bool Connected, string? AthleteName, DateTime? ConnectedAt);

public class GetStravaStatus(IUnitOfWork uow, ICurrentUser currentUser) : IUseCase<GetStravaStatusInput, GetStravaStatusOutput>
{
    public async Task<GetStravaStatusOutput> Execute(GetStravaStatusInput input, CancellationToken ct = default)
    {
        var connection = await uow.Repo<IStravaConnectionRepository>().ByUserId(currentUser.UserId, ct);
        return connection.HasValue
            ? new GetStravaStatusOutput(true, connection.Value.AthleteName, connection.Value.ConnectedAt)
            : new GetStravaStatusOutput(false, null, null);
    }
}
