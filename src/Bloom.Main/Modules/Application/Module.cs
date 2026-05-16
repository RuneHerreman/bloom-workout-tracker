using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Bloom.Application.LoggedWorkouts;
using Bloom.Application.Users;
using Bloom.Application.WorkoutTemplates;
using Bloom.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

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
        services.AddScoped<IUseCase<UpdateUserInfoInput, UpdateUserInfoOutput>, UpdateUserInfo>();
        services.AddScoped<IUseCase<UpdateTechnicalPointsInput, UpdateTechnicalPointsOutput>, UpdateTechnicalPoints>();
        services.AddScoped<IUseCase<DeleteUserInput>, DeleteUser>();
        services.AddScoped<IUseCase<ChangeUserPasswordInput>, ChangeUserPassword>();
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
        //      USERS
        services.AddScoped<IUseCase<GetCurrentUserInput, GetCurrentUserOutput>, GetCurrentUser>();
        //      EXERCISES
        services.AddScoped<IUseCase<FindExerciseByIdInput, FindExerciseByIdOutput>, FindExerciseById>();
        services.AddScoped<SearchExerciseCatalog>();
        services.AddScoped<IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput>>(sp =>
            new CachedSearchExerciseCatalogUseCase(
                sp.GetRequiredService<SearchExerciseCatalog>(),
                sp.GetRequiredService<IMemoryCache>()
            ));
        //      TEMPLATES
        services.AddScoped<IUseCase<FindUserWorkoutTemplatesInput, FindUserWorkoutTemplatesOutput>, FindUserWorkoutTemplates>();
        services.AddScoped<IUseCase<FindWorkoutTemplateByIdInput, FindWorkoutTemplateByIdOutput>, FindWorkoutTemplateById>();
        //      LOGS
        services.AddScoped<IUseCase<FindUserLoggedWorkoutsInput, FindUserLoggedWorkoutsOutput>, FindUserLoggedWorkouts>();
        services.AddScoped<IUseCase<FindLoggedWorkoutByIdInput, FindLoggedWorkoutByIdOutput>, FindLoggedWorkoutById>();
        services.AddScoped<IUseCase<GetLoggedExercisePrsInput, GetLoggedExercisePrsOutput>, GetLoggedExercisePrs>();
        services.AddScoped<IUseCase<GetLoggedExerciseVolumeInput, GetLoggedExerciseVolumeOutput>, GetLoggedExerciseVolume>();

        return services;
    }
}