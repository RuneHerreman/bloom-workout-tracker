using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Infrastructure.Persistence;
using Bloom.Infrastructure.Persistence.EntityFramework;
using Bloom.Domain.Shared.DomainEvents;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Vendors;
using Bloom.Infrastructure.Persistence.EntityFramework.Interceptors;
using Bloom.Infrastructure.Persistence.EntityFramework.Queries;
using Bloom.Infrastructure.Persistence.EntityFramework.Repositories;
using Bloom.Infrastructure.Persistence.EntityFramework.Seeders;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Main.Modules.Persistence.EntityFramework;

public static class EFCoreServices
{
    public static IServiceCollection AddEFCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddInterceptors()
            .AddSeeders()
            .AddDbContext(configuration)
            .AddRepositories()
            .AddQueries()
            .AddUnitOfWork();
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        return services
            .AddScoped<IUnitOfWork, EfCoreUnitOfWork>(sp =>
            {
                ILogger<EfCoreUnitOfWork> logger =
                    sp.GetRequiredService<ILogger<EfCoreUnitOfWork>>();

                DomainDbContext context = sp.GetRequiredService<DomainDbContext>();
                
                IUserRepository userRepository = sp.GetRequiredService<IUserRepository>();
                IWorkoutTemplateRepository workoutTemplateRepository = sp.GetRequiredService<IWorkoutTemplateRepository>();
                IExerciseRepository exerciseRepository = sp.GetRequiredService<IExerciseRepository>();
                ILoggedWorkoutRepository loggedWorkoutRepository = sp.GetRequiredService<ILoggedWorkoutRepository>();

                EfCoreUnitOfWork uow = new(context, logger);
                
                // Register repositories with the unit of work
                uow.RegisterRepository(userRepository);
                uow.RegisterRepository(workoutTemplateRepository);
                uow.RegisterRepository(exerciseRepository);
                uow.RegisterRepository(loggedWorkoutRepository);

                return uow;
            });
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IWorkoutTemplateRepository, WorkoutTemplateRepository>()
            .AddScoped<IExerciseRepository, ExerciseRepository>()
            .AddScoped<ILoggedWorkoutRepository, LoggedWorkoutRepository>();
    }

    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        return services
            .AddScoped<ISearchExerciseCatalogQuery, SearchExerciseCatalogQuery>()
            .AddScoped<IFindWorkoutTemplatesQuery, FindWorkoutTemplatesQuery>()
            .AddScoped<IFindLoggedWorkoutsQuery, FindLoggedWorkoutsQuery>();
    }
    
    public static async Task<WebApplication> SeedData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        DomainDbSeeder seeder = scope.ServiceProvider.GetRequiredService<DomainDbSeeder>();
        
        await seeder.Seed();

        return app;
    }

    private static IServiceCollection AddSeeders(
        this IServiceCollection services
    )
    {
        return services.AddScoped<DomainDbSeeder>();
    }
    
    private static IServiceCollection AddInterceptors(
        this IServiceCollection services
    )
    {
        services.AddSingleton<IDomainEventPublisher, NoOpDomainEventPublisher>();
        return services.AddScoped<PublishDomainEventsInterceptor>();
    }

    private sealed class NoOpDomainEventPublisher : IDomainEventPublisher
    {
        public Task Publish(IDomainEvent domainEvent) => Task.CompletedTask;
        public void Register(IDomainEventListener listener) { }
    }

    private static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string? databaseProvider = configuration.GetValue<string>("Database:Provider")
                                  ?? configuration.GetValue<string>("DefaultConnection:Provider");

        if (string.IsNullOrWhiteSpace(databaseProvider))
            throw new InvalidOperationException(
                "Missing database provider configuration. Configure either 'Database:Provider' or 'DefaultConnection:Provider'."
            );
        switch (databaseProvider)
        {
            case "PostgreSQL":
                services.AddDbContext<DomainDbContext, PostgresDomainDbContext>();
                services.AddDbContext<QueryDbContext, PostgresQueryDbContext>();
                break;
            default:
                throw new NotSupportedException($"Database provider '{databaseProvider}' is not supported.");
        }

        return services;
    }
}