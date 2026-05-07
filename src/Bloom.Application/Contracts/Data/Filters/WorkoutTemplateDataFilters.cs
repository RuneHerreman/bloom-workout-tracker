using System.Linq.Expressions;
using Bloom.Domain.WorkoutTemplates;

namespace Bloom.Application.Contracts.Data.Filters;

public static class WorkoutTemplateDataFilters
{
    public static Expression<Func<WorkoutTemplateData, bool>> ByProperty(Guid userId, string? name)
    {
        var cleanName = string.IsNullOrWhiteSpace(name) ? null : name.ToLower();

        if (cleanName == null)
            return template => template.UserId == userId;

        return template => template.UserId == userId && template.Name.ToLower().Contains(cleanName);
    }

    public static Expression<Func<WorkoutTemplateData, bool>> ById(WorkoutTemplateId inputId)
    {
        if (inputId.Value == Guid.Empty)
            return template => false;

        return template => template.Id == inputId.Value;
    }
}