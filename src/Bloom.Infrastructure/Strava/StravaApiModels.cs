using System.Text.Json.Serialization;

namespace Bloom.Infrastructure.Strava;

public record StravaTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_at")] long ExpiresAt,
    [property: JsonPropertyName("athlete")] StravaAthleteData? Athlete
);

public record StravaRefreshResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_at")] long ExpiresAt
);

public record StravaAthleteData(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("firstname")] string FirstName,
    [property: JsonPropertyName("lastname")] string LastName
);

public record StravaActivity(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sport_type")] string SportType,
    [property: JsonPropertyName("start_date")] DateTime StartDate,
    [property: JsonPropertyName("start_date_local")] DateTime StartDateLocal,
    [property: JsonPropertyName("elapsed_time")] int ElapsedTime,
    [property: JsonPropertyName("moving_time")] int MovingTime,
    [property: JsonPropertyName("distance")] float Distance
);

public record StravaStreams(
    [property: JsonPropertyName("latlng")] StravaStream<List<double>>? LatLng,
    [property: JsonPropertyName("altitude")] StravaStream<double>? Altitude,
    [property: JsonPropertyName("time")] StravaStream<int>? Time
);

public record StravaStream<T>(
    [property: JsonPropertyName("data")] List<T> Data
);
