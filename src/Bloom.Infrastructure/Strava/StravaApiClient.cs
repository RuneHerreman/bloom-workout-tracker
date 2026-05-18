using System.Net.Http.Json;
using System.Text.Json;
using Bloom.Domain.Strava;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.Strava;

public class StravaApiClient(
    HttpClient http,
    IOptions<StravaOptions> options,
    ILogger<StravaApiClient> logger
)
{
    private const string BaseUrl = "https://www.strava.com/api/v3";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public string BuildAuthUrl(string state)
    {
        var opts = options.Value;
        return $"https://www.strava.com/oauth/authorize" +
               $"?client_id={opts.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(opts.RedirectUri)}" +
               $"&response_type=code" +
               $"&approval_prompt=auto" +
               $"&scope=activity:read_all" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<StravaTokenResponse> ExchangeCode(string code, CancellationToken ct = default)
    {
        var opts = options.Value;
        var response = await http.PostAsync("https://www.strava.com/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = opts.ClientId,
                ["client_secret"] = opts.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
            }), ct);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StravaTokenResponse>(JsonOpts, ct)
               ?? throw new InvalidOperationException("Strava token response was null");
    }

    public async Task<string> GetValidToken(StravaConnection connection, Func<StravaConnection, Task> onRefresh, CancellationToken ct = default)
    {
        if (!connection.IsExpired())
            return connection.AccessToken;

        logger.LogInformation("Refreshing Strava token for athlete {AthleteId}", connection.StravaAthleteId);

        var opts = options.Value;
        var response = await http.PostAsync("https://www.strava.com/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = opts.ClientId,
                ["client_secret"] = opts.ClientSecret,
                ["refresh_token"] = connection.RefreshToken,
                ["grant_type"] = "refresh_token",
            }), ct);

        response.EnsureSuccessStatusCode();
        var refresh = await response.Content.ReadFromJsonAsync<StravaRefreshResponse>(JsonOpts, ct)
                      ?? throw new InvalidOperationException("Strava refresh response was null");

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(refresh.ExpiresAt).UtcDateTime;
        connection.UpdateTokens(refresh.AccessToken, refresh.RefreshToken, expiresAt);
        await onRefresh(connection);

        return refresh.AccessToken;
    }

    public async Task<List<StravaActivity>> GetActivities(string accessToken, int page, int perPage = 100, long? after = null, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/athlete/activities?per_page={perPage}&page={page}";
        if (after.HasValue) url += $"&after={after.Value}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<List<StravaActivity>>(JsonOpts, ct)
               ?? [];
    }

    public async Task<StravaActivity?> GetActivity(string accessToken, long activityId, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/activities/{activityId}");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var resp = await http.SendAsync(req, ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<StravaActivity>(JsonOpts, ct);
    }

    public async Task<StravaStreams?> GetActivityStreams(string accessToken, long activityId, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get,
            $"{BaseUrl}/activities/{activityId}/streams?keys=latlng,altitude,time&key_by_type=true");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<StravaStreams>(JsonOpts, ct);
    }
}
