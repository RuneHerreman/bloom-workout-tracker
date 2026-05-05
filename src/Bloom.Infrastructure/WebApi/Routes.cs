using Bloom.Infrastructure.WebApi.Controllers.Exercises;
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
        
        
        return userGroup;
    }

    private static RouteGroupBuilder MapExerciseRoutes(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder exerciseGroup = app.MapGroup("/exercises")
            .WithTags("Exercises");

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
        RouteGroupBuilder templateGroup = app.MapGroup("/templates");
        
        return templateGroup;
    }
}