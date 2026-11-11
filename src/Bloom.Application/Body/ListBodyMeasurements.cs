using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Body;

namespace Bloom.Application.Body;

public sealed record ListBodyMeasurementsInput(DateOnly? From = null, DateOnly? To = null);

public sealed record ListBodyMeasurementsOutput(IReadOnlyList<BodyMeasurementData> Entries);

/// <summary>Your body log, oldest first, optionally for a date range.</summary>
public class ListBodyMeasurements(IUnitOfWork uow, ICurrentUser currentUser)
    : IUseCase<ListBodyMeasurementsInput, ListBodyMeasurementsOutput>
{
    public async Task<ListBodyMeasurementsOutput> Execute(ListBodyMeasurementsInput input, CancellationToken ct = default)
    {
        var entries = await uow.Repo<IBodyMeasurementRepository>().ForUser(currentUser.UserId, input.From, input.To, ct);
        return new ListBodyMeasurementsOutput(entries.Select(e => e.ToData()).ToList());
    }
}
