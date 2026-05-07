using System.Linq.Expressions;
using Bloom.Domain.LoggedWorkouts;

namespace Bloom.Application.Contracts.Data.Filters;

public static class LoggedWorkoutDataFilters
{
    public static Expression<Func<LoggedWorkoutData, bool>> ByProperty(Guid userId)
    {
        return log => log.UserId == userId;
    }

    public static Expression<Func<LoggedWorkoutData, bool>> ById(LoggedWorkoutId inputId)
    {
        if (inputId.Value == Guid.Empty)
            return log => false;

        return log => log.Id == inputId.Value;
    }
}