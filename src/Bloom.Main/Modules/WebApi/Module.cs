using System.Text;
using System.Threading.RateLimiting;
using Bloom.Infrastructure.Auth;
using Bloom.Infrastructure.ExceptionHandlers;
using Bloom.Infrastructure.Persistence;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Bloom.Infrastructure.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Bloom.Infrastructure.Persistence.EntityFramework.Seeders;
using Bloom.Main.Modules.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Main.Modules.WebApi;

public static class Module
{
    public static IServiceCollection AddWebApiModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHealthChecks();
        services.AddProblemDetails();
        services.AddExceptionHandler<BloomExceptionHandler>();

        services.AddHttpContextAccessor();
        services.AddControllers();

        // Persist DataProtection keys so encrypted-at-rest data (Strava tokens) and
        // in-flight OAuth state survive container restarts.
        var dataProtection = services.AddDataProtection().SetApplicationName("bloom");
        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        // The app runs behind nginx (and Tailscale serve in production), which terminate
        // TLS and proxy plain HTTP. Honor their X-Forwarded-* headers so Request.IsHttps
        // and the client IP reflect the original request.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = null;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Missing '{JwtOptions.SectionName}' configuration section.");

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                };

                // Read token from the HttpOnly cookie so JS cannot access it
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(jwtOptions.CookieName, out var token))
                            context.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", policy =>
            {
                policy
                    .SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddRateLimiter(options =>
        {
            // Partition per client IP (resolved via forwarded headers) so one client
            // hammering login can't lock everyone else out.
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Enter a valid JWT bearer token."
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
                var requiresAuth = endpointMetadata.OfType<IAuthorizeData>().Any();
                var allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();

                if (!requiresAuth || allowsAnonymous)
                {
                    return Task.CompletedTask;
                }

                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer")] = []
                    }
                ];

                return Task.CompletedTask;
            });
        });
        services.AddAuthorization();
        services.AddValidation();

        return services;
    }

    public static async Task<WebApplication> UseWebApiModule(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DomainDbContext>();
        await context.Database.MigrateAsync();

        await app.SeedData();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "Bloom Workout Tracker API";
                options.Authentication = new ScalarAuthenticationOptions
                {
                    PreferredSecuritySchemes = ["Bearer"]
                };
            });
        }

        app.UseForwardedHeaders();

        app.Use(async (ctx, next) =>
        {
            ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
            ctx.Response.Headers["X-Frame-Options"] = "DENY";
            ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            await next();
        });

        app.UseExceptionHandler();
        app.UseCors("AllowLocalhost");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapHealthChecks("/health");
        app.MapRoutes();

        return app;
    }
}
