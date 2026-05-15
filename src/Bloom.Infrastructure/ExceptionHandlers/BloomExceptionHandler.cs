using Bloom.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.ExceptionHandlers;

public sealed class BloomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, detail) = exception switch
        {
            UserAlreadyExistsException e => (StatusCodes.Status409Conflict, e.Message),
            InvalidCredentialsException e => (StatusCodes.Status401Unauthorized, e.Message),

            // Unify not-found and access-denied → 404 to prevent resource enumeration
            UserNotFoundException
                or ExerciseNotFoundException
                or LoggedWorkoutNotFoundException
                or LoggedWorkoutAccessDeniedException
                or WorkoutTemplateNotFoundException
                or WorkoutTemplateAccessDeniedException => (StatusCodes.Status404NotFound, "Resource not found."),

            BloomGeneralException e => (StatusCodes.Status400BadRequest, e.Message),

            _ => (0, null)
        };

        if (status == 0) return false;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = status, Title = ReasonPhrases(status), Detail = detail },
            cancellationToken);

        return true;
    }

    private static string ReasonPhrases(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Error"
    };
}
