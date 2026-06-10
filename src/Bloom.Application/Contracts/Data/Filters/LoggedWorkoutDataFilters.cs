using System.Linq.Expressions;
using Bloom.Domain.LoggedWorkouts;

namespace Bloom.Application.Contracts.Data.Filters;

public static class LoggedWorkoutDataFilters
{
    public static Expression<Func<LoggedWorkoutData, bool>> ByProperty(
        Guid userId,
        string? name = null,
        DateTime? from = null,
        DateTime? to = null,
        string? gear = null)
    {
        var cleanName = string.IsNullOrWhiteSpace(name) ? null : name.ToLower();
        var cleanGear = string.IsNullOrWhiteSpace(gear) ? null : gear.Trim();

        return log =>
            log.UserId == userId &&
            (cleanName == null || log.Name.ToLower().Contains(cleanName)) &&
            (from == null || log.LoggedAt >= from) &&
            (to == null || log.LoggedAt <= to) &&
            (cleanGear == null || log.LoggedExercises.Any(e => e.Gear.Contains(cleanGear)));
    }

    public static Expression<Func<LoggedWorkoutData, bool>> ById(LoggedWorkoutId inputId)
    {
        if (inputId.Value == Guid.Empty)
            return log => false;

        return log => log.Id == inputId.Value;
    }
}