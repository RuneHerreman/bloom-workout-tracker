using Bloom.Infrastructure.WebApi.Controllers.Exercises;
using Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;
using Bloom.Infrastructure.WebApi.Controllers.Users;
using Bloom.Infrastructure.WebApi.Controllers.WorkoutTemplates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bloom.Infrastructure.WebApi;

public static class Routes
{
    public static IEndpointRouteBuilder MapRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder webApi = app.MapGroup("/api");

        webApi.MapUserRoutes();
        webApi.MapExerciseRoutes();
        webApi.MapTemplateRoutes();
        webApi.MapLogRoutes();

        return app;
    }

    private static RouteGroupBuilder MapUserRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder userGroup = app.MapGroup("/users")
            .WithTags("Users");

        userGroup.MapPost("/register", RegisterUserController.Invoke)
            .WithName(nameof(RegisterUserController))
            .WithDescription("Register a new user.")
            .AllowAnonymous();

        userGroup.MapPost("/login", LoginUserController.Invoke)
            .WithName(nameof(LoginUserController))
            .WithDescription("Authenticate a user with email and password.")
            .AllowAnonymous();

        return userGroup;
    }

    private static RouteGroupBuilder MapExerciseRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder exerciseGroup = app.MapGroup("/exercises")
            .WithTags("Exercises")
            .RequireAuthorization();

        exerciseGroup.MapGet("/{ExerciseId:guid}", FindExerciseByIdController.Invoke)
            .WithName(nameof(FindExerciseByIdController))
            .WithDescription("Find an exercise by its unique identifier.");

        exerciseGroup.MapGet("", SearchExerciseCatalogController.Invoke)
            .WithName(nameof(SearchExerciseCatalogController))
            .WithDescription("Search for exercises based on specified criteria.");

        return exerciseGroup;
    }


    private static RouteGroupBuilder MapTemplateRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder templateGroup = app.MapGroup("/templates")
            .WithTags("Templates")
            .RequireAuthorization();

        templateGroup.MapGet("", GetUserWorkoutTemplatesController.Invoke)
            .WithName(nameof(GetUserWorkoutTemplatesController))
            .WithDescription("Get workout templates for the authenticated user with optional name search.");

        templateGroup.MapGet("/{TemplateId:guid}", FindWorkoutTemplateByIdController.Invoke)
            .WithName(nameof(FindWorkoutTemplateByIdController))
            .WithDescription("Find a workout template by its unique identifier.");

        templateGroup.MapPost("", CreateWorkoutTemplateController.Invoke)
            .WithName(nameof(CreateWorkoutTemplateController))
            .WithDescription("Create a new workout template.");

        templateGroup.MapPut("/{TemplateId:guid}", UpdateWorkoutTemplateController.Invoke)
            .WithName(nameof(UpdateWorkoutTemplateController))
            .WithDescription("Update an existing workout template by id (full overwrite).");

        templateGroup.MapDelete("/{TemplateId:guid}", DeleteWorkoutTemplateController.Invoke)
            .WithName(nameof(DeleteWorkoutTemplateController))
            .WithDescription("Delete a workout template by id.");

        return templateGroup;
    }

    private static RouteGroupBuilder MapLogRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder logGroup = app.MapGroup("/logs")
            .WithTags("Logs")
            .RequireAuthorization();

        logGroup.MapGet("", GetUserLoggedWorkoutsController.Invoke)
            .WithName(nameof(GetUserLoggedWorkoutsController))
            .WithDescription("Get workout logs for the authenticated user.");

        logGroup.MapGet("/{LoggedWorkoutId:guid}", FindLoggedWorkoutByIdController.Invoke)
            .WithName(nameof(FindLoggedWorkoutByIdController))
            .WithDescription("Find a workout log by its unique identifier.");

        logGroup.MapPost("", CreateLoggedWorkoutController.Invoke)
            .WithName(nameof(CreateLoggedWorkoutController))
            .WithDescription("Create a new workout log.");

        logGroup.MapPut("/{LoggedWorkoutId:guid}", UpdateLoggedWorkoutController.Invoke)
            .WithName(nameof(UpdateLoggedWorkoutController))
            .WithDescription("Update an existing workout log by id (full overwrite).");

        logGroup.MapDelete("/{LoggedWorkoutId:guid}", DeleteLoggedWorkoutController.Invoke)
            .WithName(nameof(DeleteLoggedWorkoutController))
            .WithDescription("Delete a workout log by id.");

        return logGroup;
    }
}
