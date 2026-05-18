using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Strava;

public readonly record struct StravaConnectionId(Guid Value) : IEntityId;

public class StravaConnection : AggregateRoot<StravaConnectionId>
{
    public UserId UserId { get; private set; }
    public long StravaAthleteId { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public string AthleteName { get; private set; } = string.Empty;
    public DateTime ConnectedAt { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    private StravaConnection() { }

    private StravaConnection(
        StravaConnectionId id,
        UserId userId,
        long stravaAthleteId,
        string accessToken,
        string refreshToken,
        DateTime expiresAt,
        string athleteName,
        DateTime connectedAt) : base(id)
    {
        UserId = userId;
        StravaAthleteId = stravaAthleteId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        AthleteName = athleteName;
        ConnectedAt = connectedAt;
    }

    public static StravaConnection Create(
        UserId userId,
        long stravaAthleteId,
        string accessToken,
        string refreshToken,
        DateTime expiresAt,
        string athleteName)
    {
        return new StravaConnection(
            EntityId.New<StravaConnectionId>(),
            userId,
            stravaAthleteId,
            accessToken,
            refreshToken,
            expiresAt,
            athleteName,
            DateTime.UtcNow
        );
    }

    public void UpdateLastSyncedAt(DateTime syncedAt) => LastSyncedAt = syncedAt;

    public void UpdateTokens(string accessToken, string refreshToken, DateTime expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt.AddMinutes(-5);

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(UserId);
        Asserts.EnsureNotEmpty(AccessToken);
        Asserts.EnsureNotEmpty(RefreshToken);
    }
}
