using System.Text;
using Bloom.Infrastructure.Persistence;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Bloom.Infrastructure.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Bloom.Infrastructure.Persistence.EntityFramework.Seeders;
using Bloom.Main.Modules.Persistence.EntityFramework;

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
        
        // Repositories
        services.AddHttpContextAccessor();
        
        // Controllers and OpenAPI
        services.AddControllers();
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
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

                // Override any pre-existing empty security entries so auth is required in Scalar.
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
        // Auto-create tables
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DomainDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Seed data
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

        app.UseCors("AllowLocalhost");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRoutes();
        
        return app;
    }
}