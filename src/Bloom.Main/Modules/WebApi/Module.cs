using System.Text;
using Bloom.Infrastructure.Persistence;
using Bloom.Infrastructure.WebApi;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

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
        services.AddOpenApi();
        services.AddAuthorization();
        
        services.AddValidation();
        
        return services;
    }

    public static async Task<WebApplication> UseWebApiModule(this WebApplication app)
    {
        // Auto-create tables
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BloomDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseCors("AllowLocalhost");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRoutes();
        
        return app;
    }
}