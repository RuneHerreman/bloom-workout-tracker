using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Bloom.Application.LoggedWorkouts;
using Bloom.Application.Users;
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
        //      USERS
        services.AddScoped<IUseCase<RegisterUserInput, RegisterUserOutput>, RegisterUser>();
        services.AddScoped<IUseCase<LoginUserInput, LoginUserOutput>, LoginUser>();
        //      TEMPLATES
        services.AddScoped<IUseCase<CreateWorkoutTemplateInput, CreateWorkoutTemplateOutput>, CreateWorkoutTemplate>();
        services.AddScoped<IUseCase<UpdateWorkoutTemplateInput, UpdateWorkoutTemplateOutput>, UpdateWorkoutTemplate>();
        services.AddScoped<IUseCase<DeleteWorkoutTemplateInput>, DeleteWorkoutTemplate>();
        //      LOGS
        services.AddScoped<IUseCase<CreateLoggedWorkoutInput, CreateLoggedWorkoutOutput>, CreateLoggedWorkout>();
        services.AddScoped<IUseCase<UpdateLoggedWorkoutInput, UpdateLoggedWorkoutOutput>, UpdateLoggedWorkout>();
        services.AddScoped<IUseCase<DeleteLoggedWorkoutInput>, DeleteLoggedWorkout>();



        // ------------------------------------------------------------------
        // -------------------- Register query use cases --------------------
        // ------------------------------------------------------------------
        //      EXERCISES
        services.AddScoped<IUseCase<FindExerciseByIdInput, FindExerciseByIdOutput>, FindExerciseById>();
        services.AddScoped<IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput>, SearchExerciseCatalog>();
        //      TEMPLATES
        services.AddScoped<IUseCase<FindUserWorkoutTemplatesInput, FindUserWorkoutTemplatesOutput>, FindUserWorkoutTemplates>();
        services.AddScoped<IUseCase<FindWorkoutTemplateByIdInput, FindWorkoutTemplateByIdOutput>, FindWorkoutTemplateById>();
        //      LOGS
        services.AddScoped<IUseCase<FindUserLoggedWorkoutsInput, FindUserLoggedWorkoutsOutput>, FindUserLoggedWorkouts>();
        services.AddScoped<IUseCase<FindLoggedWorkoutByIdInput, FindLoggedWorkoutByIdOutput>, FindLoggedWorkoutById>();

        return services;
    }
}