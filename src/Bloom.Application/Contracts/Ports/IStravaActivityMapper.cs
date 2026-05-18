using Bloom.Application.LoggedWorkouts;

namespace Bloom.Application.Contracts.Ports;

public interface IStravaActivityMapper
{
    Task<CreateLoggedWorkoutInput?> Map(StravaActivityResult activity, StravaActivityStreamsResult? streams, CancellationToken ct = default);
}
