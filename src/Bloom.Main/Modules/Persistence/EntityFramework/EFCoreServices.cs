using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.LogBook;
using Bloom.Domain.Templates;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence;
using Bloom.Infrastructure.Persistence.EntityFramework;
using Bloom.Infrastructure.Persistence.EntityFramework.Queries;
using Bloom.Infrastructure.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Main.Modules.Persistence.EntityFramework;

public static class EFCoreServices
{
    public static IServiceCollection AddEFCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
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

                BloomDbContext context = sp.GetRequiredService<BloomDbContext>();

                EfCoreUnitOfWork uow = new(context, logger);
                
                // Register repositories with the unit of work
                uow.RegisterRepository(sp.GetRequiredService<IUserRepository>());
                uow.RegisterRepository(sp.GetRequiredService<IExerciseRepository>());
                uow.RegisterRepository(sp.GetRequiredService<ILogBookRepository>());
                uow.RegisterRepository(sp.GetRequiredService<IWorkoutTemplateRepository>());

                return uow;
            });
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IExerciseRepository, ExerciseRepository>()
            .AddScoped<ILogBookRepository, LogBookRepository>()
            .AddScoped<IWorkoutTemplateRepository, WorkoutTemplateRepository>();
    }

    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        return services
            .AddScoped<IGetAllExercisesQuery, GetAllExercisesQuery>()
            .AddScoped<IGetAllUserTemplatesQuery, GetAllUserTemplatesQuery>();
    }

    private static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;
        return services.AddDbContext<BloomDbContext>(options => options.UseNpgsql(connectionString));
    }
}