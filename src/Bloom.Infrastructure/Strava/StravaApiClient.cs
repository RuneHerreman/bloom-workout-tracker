using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bloom.Application.Contracts.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.Strava;

public class StravaApiClient(
    HttpClient http,
    IOptions<StravaOptions> options,
    ILogger<StravaApiClient> logger
) : IStravaClient
{
    private const string BaseUrl = "https://www.strava.com/api/v3";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public string BuildAuthUrl(string state)
    {
        var opts = options.Value;
        return "https://www.strava.com/oauth/authorize" +
               $"?client_id={opts.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(opts.RedirectUri)}" +
               $"&response_type=code" +
               $"&approval_prompt=auto" +
               $"&scope=activity:read_all" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<StravaTokenExchangeResult> ExchangeCode(string code, CancellationToken ct = default)
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
        var raw = await response.Content.ReadFromJsonAsync<StravaTokenResponse>(JsonOpts, ct)
                  ?? throw new InvalidOperationException("Strava token response was null");

        var athlete = raw.Athlete ?? throw new InvalidOperationException("No athlete data in Strava response");
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(raw.ExpiresAt).UtcDateTime;

        return new StravaTokenExchangeResult(
            raw.AccessToken,
            raw.RefreshToken,
            expiresAt,
            athlete.Id,
            $"{athlete.FirstName} {athlete.LastName}".Trim()
        );
    }

    public async Task<StravaTokenRefreshResult?> TryRefreshToken(string refreshToken, CancellationToken ct = default)
    {
        var opts = options.Value;
        try
        {
            var response = await http.PostAsync("https://www.strava.com/oauth/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = opts.ClientId,
                    ["client_secret"] = opts.ClientSecret,
                    ["refresh_token"] = refreshToken,
                    ["grant_type"] = "refresh_token",
                }), ct);

            response.EnsureSuccessStatusCode();
            var raw = await response.Content.ReadFromJsonAsync<StravaRefreshResponse>(JsonOpts, ct)
                      ?? throw new InvalidOperationException("Strava refresh response was null");

            return new StravaTokenRefreshResult(
                raw.AccessToken,
                raw.RefreshToken,
                DateTimeOffset.FromUnixTimeSeconds(raw.ExpiresAt).UtcDateTime
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh Strava token");
            return null;
        }
    }

    public async Task<List<StravaActivityResult>> GetActivities(string accessToken, int page, int perPage = 100, long? after = null, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/athlete/activities?per_page={perPage}&page={page}";
        if (after.HasValue) url += $"&after={after.Value}";

        using var resp = await SendWithRateLimitRetry(
            () => AuthorizedGet(accessToken, url),
            ct);

        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadFromJsonAsync<List<StravaActivity>>(JsonOpts, ct) ?? [];
        return raw.Select(ToResult).ToList();
    }

    public async Task<StravaActivityStreamsResult?> GetActivityStreams(string accessToken, long activityId, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/activities/{activityId}/streams?keys=latlng,altitude,time&key_by_type=true";

        using var resp = await SendWithRateLimitRetry(
            () => AuthorizedGet(accessToken, url),
            ct);

        if (!resp.IsSuccessStatusCode) return null;

        var raw = await resp.Content.ReadFromJsonAsync<StravaStreams>(JsonOpts, ct);
        if (raw is null) return null;

        var latLng = raw.LatLng?.Data ?? [];
        var times = raw.Time?.Data;
        if (latLng.Count == 0 || times is null || times.Count == 0) return null;

        return new StravaActivityStreamsResult(latLng, raw.Altitude?.Data, times);
    }

    private async Task<HttpResponseMessage> SendWithRateLimitRetry(Func<HttpRequestMessage> requestFactory, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            using var req = requestFactory();
            var resp = await http.SendAsync(req, ct);

            if (resp.StatusCode != HttpStatusCode.TooManyRequests)
                return resp;

            var delay = resp.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
            logger.LogWarning("Strava rate limit hit. Waiting {Delay}s before retry (attempt {Attempt}/3)", delay.TotalSeconds, attempt);
            await Task.Delay(delay, ct);
        }

        throw new HttpRequestException("Strava rate limit exceeded after 3 retries");
    }

    private static HttpRequestMessage AuthorizedGet(string accessToken, string url) =>
        new(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };

    private static StravaActivityResult ToResult(StravaActivity a) =>
        new(a.Id, a.Name, a.SportType, a.StartDate, a.ElapsedTime, a.MovingTime, a.Distance);
}
