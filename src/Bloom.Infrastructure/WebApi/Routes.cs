using Bloom.Infrastructure.WebApi.Controllers.Exercises;
using Bloom.Infrastructure.WebApi.Controllers.Users;
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
        return app;
    }

    private static RouteGroupBuilder MapUserRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder userGroup = app.MapGroup("/users");
        
        userGroup.MapPost("/register", RegisterUserController.Invoke)
            .WithTags("Users")
            .WithDescription("Registers a new user and returns an authentication token.");
        
        userGroup.MapPost("/login", LoginUserController.Invoke)
            .WithTags("Users")
            .WithDescription("Authenticates a user and returns an authentication token.");
        
        return userGroup;
    }

    private static RouteGroupBuilder MapExerciseRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder exerciseGroup = app.MapGroup("/exercises");

        exerciseGroup.MapGet("/", GetAllExercisesController.Invoke)
            .WithTags("Exercises")
            .WithDescription("Retrieves a list of all exercises.");
        
        return exerciseGroup;
    }

}