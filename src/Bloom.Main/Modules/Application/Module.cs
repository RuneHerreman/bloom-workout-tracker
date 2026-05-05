using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;

namespace Bloom.Main.Modules.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // --------------------------------------------------------------------
        // -------------------- Register command use cases --------------------
        // --------------------------------------------------------------------
        
        // ------------------------------------------------------------------
        // -------------------- Register query use cases --------------------
        // ------------------------------------------------------------------
        services.AddScoped<IUseCase<FindExerciseByIdInput, FindExerciseByIdOutput>, FindExerciseById>();
        services.AddScoped<IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput>, SearchExerciseCatalog>();
        
        return services;
    }
}