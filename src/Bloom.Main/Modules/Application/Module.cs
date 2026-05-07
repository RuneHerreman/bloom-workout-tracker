using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Bloom.Application.WorkoutTemplates;

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
        //      TEMPLATES
        services.AddScoped<IUseCase<CreateWorkoutTemplateInput, CreateWorkoutTemplateOutput>, CreateWorkoutTemplate>();
        
        
        
        // ------------------------------------------------------------------
        // -------------------- Register query use cases --------------------
        // ------------------------------------------------------------------
        //      EXERCISES
        services.AddScoped<IUseCase<FindExerciseByIdInput, FindExerciseByIdOutput>, FindExerciseById>();
        services.AddScoped<IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput>, SearchExerciseCatalog>();
        
        return services;
    }
}