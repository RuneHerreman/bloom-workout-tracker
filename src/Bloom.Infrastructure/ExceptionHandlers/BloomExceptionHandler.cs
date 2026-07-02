using Bloom.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

            // Unique constraint violation (e.g. two concurrent registrations racing past the exists-check)
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } }
                => (StatusCodes.Status409Conflict, "Resource already exists."),

            ExerciseAlreadyExistsException e => (StatusCodes.Status409Conflict, e.Message),
            InvalidCredentialsException e => (StatusCodes.Status401Unauthorized, e.Message),
            InvalidWorkoutTemplateException e => (StatusCodes.Status400BadRequest, e.Message),

            // Unify not-found and access-denied → same 404 to prevent resource enumeration
            UserNotFoundException
                or UserAccessDeniedException
                or ExerciseNotFoundException
                or ExerciseAccessDeniedException
                or LoggedWorkoutNotFoundException
                or LoggedWorkoutAccessDeniedException
                or WorkoutTemplateNotFoundException
                or WorkoutTemplateAccessDeniedException
                or StravaConnectionNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),

            BloomGeneralException e => (StatusCodes.Status400BadRequest, e.Message),

            _ => (0, null)
        };

        if (status == 0) return false;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = status, Title = ToTitle(status), Detail = detail },
            cancellationToken);

        return true;
    }

    private static string ToTitle(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Error"
    };
}
