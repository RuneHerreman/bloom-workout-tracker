using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Persistence;
using Bloom.Infrastructure.Persistence.EntityFramework;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
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
                // uow.RegisterRepository(sp.GetRequiredService<IUserRepository>());

                return uow;
            });
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            // .AddScoped<IExerciseRepository, ExerciseRepository>();
    }

    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        return services
            // .AddScoped<IGetAllUserTemplatesQuery, GetAllUserTemplatesQuery>();
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