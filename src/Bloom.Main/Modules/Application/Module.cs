using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Bloom.Application.LoggedWorkouts;
using Bloom.Application.Strava;
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
        services.AddScoped<IUseCase<UpdateUserGearInput, UpdateUserGearOutput>, UpdateUserGear>();
        services.AddScoped<IUseCase<DeleteUserInput>, DeleteUser>();
        services.AddScoped<IUseCase<ChangeUserPasswordInput>, ChangeUserPassword>();
        //      EXERCISES
        services.AddSingleton<ExerciseCatalogCacheVersion>();
        services.AddScoped<CreateCustomExercise>();
        services.AddScoped<IUseCase<CreateCustomExerciseInput, CreateCustomExerciseOutput>>(sp =>
            new CacheInvalidatingCreateCustomExerciseUseCase(
                sp.GetRequiredService<CreateCustomExercise>(),
                sp.GetRequiredService<ExerciseCatalogCacheVersion>(),
                sp.GetRequiredService<ICurrentUser>()
            ));
        services.AddScoped<UpdateCustomExercise>();
        services.AddScoped<IUseCase<UpdateCustomExerciseInput, UpdateCustomExerciseOutput>>(sp =>
            new CacheInvalidatingUpdateCustomExerciseUseCase(
                sp.GetRequiredService<UpdateCustomExercise>(),
                sp.GetRequiredService<ExerciseCatalogCacheVersion>(),
                sp.GetRequiredService<ICurrentUser>()
            ));
        services.AddScoped<DeleteCustomExercise>();
        services.AddScoped<IUseCase<DeleteCustomExerciseInput>>(sp =>
            new CacheInvalidatingDeleteCustomExerciseUseCase(
                sp.GetRequiredService<DeleteCustomExercise>(),
                sp.GetRequiredService<ExerciseCatalogCacheVersion>(),
                sp.GetRequiredService<ICurrentUser>()
            ));
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
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<ICurrentUser>(),
                sp.GetRequiredService<ExerciseCatalogCacheVersion>()
            ));
        //      TEMPLATES
        services.AddScoped<IUseCase<FindUserWorkoutTemplatesInput, FindUserWorkoutTemplatesOutput>, FindUserWorkoutTemplates>();
        services.AddScoped<IUseCase<FindWorkoutTemplateByIdInput, FindWorkoutTemplateByIdOutput>, FindWorkoutTemplateById>();
        //      LOGS
        services.AddScoped<IUseCase<FindUserLoggedWorkoutsInput, FindUserLoggedWorkoutsOutput>, FindUserLoggedWorkouts>();
        services.AddScoped<IUseCase<FindLoggedWorkoutByIdInput, FindLoggedWorkoutByIdOutput>, FindLoggedWorkoutById>();
        services.AddScoped<IUseCase<GetLoggedExercisePrsInput, GetLoggedExercisePrsOutput>, GetLoggedExercisePrs>();
        services.AddScoped<IUseCase<GetLoggedExerciseVolumeInput, GetLoggedExerciseVolumeOutput>, GetLoggedExerciseVolume>();
        //      STRAVA
        services.AddScoped<IUseCase<GetStravaStatusInput, GetStravaStatusOutput>, GetStravaStatus>();
        services.AddScoped<IUseCase<ConnectStravaInput, ConnectStravaOutput>, ConnectStrava>();
        services.AddScoped<IUseCase<DisconnectStravaInput>, DisconnectStrava>();
        services.AddScoped<IUseCase<ImportAllStravaActivitiesInput, ImportAllStravaActivitiesOutput>, ImportAllStravaActivities>();
        services.AddScoped<IUseCase<SyncStravaActivitiesInput, SyncStravaActivitiesOutput>, SyncStravaActivities>();

        return services;
    }
}