namespace Bloom.Application.Contracts.Ports;

public record StravaTokenExchangeResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    long AthleteId,
    string AthleteName
);

public record StravaTokenRefreshResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record StravaActivityResult(
    long Id,
    string Name,
    string SportType,
    DateTime StartDate,
    int ElapsedTime,
    int MovingTime,
    float Distance
);

public record StravaActivityStreamsResult(
    List<List<double>> LatLng,
    List<double>? Altitude,
    List<int> Time
);

public interface IStravaClient
{
    string BuildAuthUrl(string state);
    Task<StravaTokenExchangeResult> ExchangeCode(string code, CancellationToken ct = default);
    Task<StravaTokenRefreshResult?> TryRefreshToken(string refreshToken, CancellationToken ct = default);
    Task<List<StravaActivityResult>> GetActivities(string accessToken, int page, int perPage = 100, long? after = null, CancellationToken ct = default);
    Task<StravaActivityStreamsResult?> GetActivityStreams(string accessToken, long activityId, CancellationToken ct = default);
}
