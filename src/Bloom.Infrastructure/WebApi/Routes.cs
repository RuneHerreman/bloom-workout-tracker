using Bloom.Infrastructure.WebApi.Controllers.Exercises;
using Bloom.Infrastructure.WebApi.Controllers.Templates;
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
        webApi.MapTemplateRoutes();
        
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
            .WithDescription("Retrieves a list of all exercises.")
            .RequireAuthorization();
        
        return exerciseGroup;
    }


    private static RouteGroupBuilder MapTemplateRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder templateGroup = app.MapGroup("/templates");

        templateGroup.MapPost("/", CreateTemplateController.Invoke)
            .WithTags("Templates")
            .WithDescription("Creates a new workout template for the authenticated user.")
            .RequireAuthorization();
        
        templateGroup.MapGet("/", GetAllUserTemplatesController.Invoke)
            .WithTags("Templates")
            .WithDescription("Retrieves a list of workout templates for the authenticated user.")
            .RequireAuthorization();
        
        return templateGroup;
    }
}