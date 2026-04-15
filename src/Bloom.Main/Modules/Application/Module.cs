using Bloom.Application.Contracts.Data;
using Bloom.Application.Contracts.Data.Templates;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Templates;
using Bloom.Application.Users;
using Bloom.Domain.Templates;

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
        services.AddScoped<IUseCase<RegisterUserInput, RegisterUserOutput>, RegisterUser>();
        services.AddScoped<IUseCase<LoginUserInput, LoginUserOutput>, LoginUser>();
        
        // Templates
        services.AddScoped<IUseCase<CreateWorkoutTemplateInput, WorkoutTemplateId>, CreateTemplate>();
        
        
        // ------------------------------------------------------------------
        // -------------------- Register query use cases --------------------
        // ------------------------------------------------------------------
        services.AddScoped<IUseCase<GetAlLExercisesInput, IEnumerable<ExerciseData>>, GetAllExercises>();
        services.AddScoped<IUseCase<GetAllUserTemplatesInput, IReadOnlyList<WorkoutTemplateData>>, GetAllUserTemplates>();
        
        return services;
    }
}