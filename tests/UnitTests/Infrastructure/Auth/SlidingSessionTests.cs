using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Infrastructure.Auth;

public sealed class SlidingSessionTests : ApplicationTestBase
{
    private sealed class ManualClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
        public void Advance(TimeSpan by) => Now += by;
    }

    private static readonly JwtOptions Options = new()
    {
        Issuer = "bloom",
        Audience = "bloom-client",
        Key = "unit-test-signing-key-at-least-32-chars",
        ExpiryMinutes = 60,
        RememberMeDays = 30
    };

    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero));
    private readonly StubPasswordHasher _passwordHasher = new();
    private readonly JwtTokenIssuer _issuer;

    public SlidingSessionTests()
    {
        _issuer = new JwtTokenIssuer(Microsoft.Extensions.Options.Options.Create(Options), _clock);
    }

    private async Task<User> SeedUser()
    {
        var user = User.Create(
            "user@example.com", "alice", _passwordHasher.HashPassword("secret"),
            "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    /// <summary>Validates like the API does (same parameters, inbound claim mapping on), at the manual clock's time.</summary>
    private ClaimsPrincipal Validate(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.Key)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero,
            LifetimeValidator = (notBefore, expires, _, _) =>
                notBefore <= _clock.Now.UtcDateTime && _clock.Now.UtcDateTime < expires
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
    }

    private TokenValidatedContext ContextFor(string token)
    {
        var services = new ServiceCollection()
            .AddSingleton<TimeProvider>(_clock)
            .AddSingleton<IUseCase<RenewSessionInput, RenewSessionOutput>>(
                new RenewSession(UnitOfWork, _issuer, CreateLogger<RenewSession>()))
            .BuildServiceProvider();

        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.Headers.Cookie = $"{Options.CookieName}={token}";

        return new TokenValidatedContext(
            http,
            new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)),
            new JwtBearerOptions())
        {
            Principal = Validate(token)
        };
    }

    private static string? RenewedToken(HttpContext http)
    {
        var header = http.Response.Headers.SetCookie.ToString();
        var prefix = $"{Options.CookieName}=";
        if (!header.StartsWith(prefix)) return null;
        var value = header[prefix.Length..].Split(';')[0];
        return value.Length == 0 ? null : value;
    }

    [Fact]
    public async Task Issue_ShouldUseIdleTimeoutForNormalAndRememberMeLifetimeForPersistent()
    {
        var user = await SeedUser();

        var normal = new JwtSecurityTokenHandler().ReadJwtToken(_issuer.Issue(SessionTicket.For(user, false)));
        var remembered = new JwtSecurityTokenHandler().ReadJwtToken(_issuer.Issue(SessionTicket.For(user, true)));

        Assert.Equal(_clock.Now.UtcDateTime.AddMinutes(60), normal.ValidTo);
        Assert.Equal(_clock.Now.UtcDateTime.AddDays(30), remembered.ValidTo);
    }

    [Fact]
    public async Task DueRenewal_ForFreshToken_ShouldBeNull()
    {
        var user = await SeedUser();
        var principal = Validate(_issuer.Issue(SessionTicket.For(user, false)));

        _clock.Advance(TimeSpan.FromMinutes(29));

        Assert.Null(SlidingSession.DueRenewal(principal, _clock.Now, Options));
    }

    [Fact]
    public async Task DueRenewal_PastHalfLifetime_ShouldCarryUserStampAndRememberMe()
    {
        var user = await SeedUser();
        var principal = Validate(_issuer.Issue(SessionTicket.For(user, true)));

        _clock.Advance(TimeSpan.FromMinutes(31));
        var renewal = SlidingSession.DueRenewal(principal, _clock.Now, Options);

        Assert.NotNull(renewal);
        Assert.Equal(user.Id, renewal.UserId);
        Assert.Equal(SessionTicket.StampOf(user), renewal.Stamp);
        Assert.True(renewal.Persistent);
    }

    [Fact]
    public async Task OnTokenValidated_ForFreshToken_ShouldNotTouchTheCookie()
    {
        var user = await SeedUser();
        var context = ContextFor(_issuer.Issue(SessionTicket.For(user, false)));

        await SlidingSession.OnTokenValidated(context, Options);

        Assert.Null(context.Result);
        Assert.Null(RenewedToken(context.HttpContext));
    }

    [Fact]
    public async Task OnTokenValidated_PastHalfLifetime_ShouldReissueSessionCookie()
    {
        var user = await SeedUser();
        var token = _issuer.Issue(SessionTicket.For(user, false));
        _clock.Advance(TimeSpan.FromMinutes(45));
        var context = ContextFor(token);

        await SlidingSession.OnTokenValidated(context, Options);

        var renewed = RenewedToken(context.HttpContext);
        Assert.NotNull(renewed);
        Assert.Equal(_clock.Now.UtcDateTime.AddMinutes(60), new JwtSecurityTokenHandler().ReadJwtToken(renewed).ValidTo);
        // Without "Remember me" the cookie stays a browser-session cookie.
        Assert.DoesNotContain("expires=", context.HttpContext.Response.Headers.SetCookie.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OnTokenValidated_RememberMe_ShouldReissuePersistentCookie()
    {
        var user = await SeedUser();
        var token = _issuer.Issue(SessionTicket.For(user, true));
        _clock.Advance(TimeSpan.FromHours(2));
        var context = ContextFor(token);

        await SlidingSession.OnTokenValidated(context, Options);

        Assert.NotNull(RenewedToken(context.HttpContext));
        Assert.Contains("expires=", context.HttpContext.Response.Headers.SetCookie.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OnTokenValidated_AfterPasswordChange_ShouldFailAndClearCookie()
    {
        var user = await SeedUser();
        var token = _issuer.Issue(SessionTicket.For(user, false));
        user.ChangePassword(_passwordHasher.HashPassword("changed"));
        await UserRepository.Save(user);
        _clock.Advance(TimeSpan.FromMinutes(31));
        var context = ContextFor(token);

        await SlidingSession.OnTokenValidated(context, Options);

        Assert.NotNull(context.Result);
        Assert.False(context.Result.Succeeded);
        Assert.Null(RenewedToken(context.HttpContext));
        Assert.Contains($"{Options.CookieName}=;", context.HttpContext.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task ActiveUser_RequestingEvery20Minutes_ShouldStaySignedInForHours()
    {
        var user = await SeedUser();
        var token = _issuer.Issue(SessionTicket.For(user, false));

        for (var i = 0; i < 15; i++) // five hours of activity
        {
            _clock.Advance(TimeSpan.FromMinutes(20));
            var context = ContextFor(token); // throws if the token had expired
            await SlidingSession.OnTokenValidated(context, Options);
            Assert.Null(context.Result);
            token = RenewedToken(context.HttpContext) ?? token;
        }
    }

    [Fact]
    public async Task IdleUser_ShouldExpireAfterTheIdleTimeout()
    {
        var user = await SeedUser();
        var token = _issuer.Issue(SessionTicket.For(user, false));

        _clock.Advance(TimeSpan.FromMinutes(61));

        Assert.ThrowsAny<SecurityTokenException>(() => Validate(token));
    }
}
