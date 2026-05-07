using System.Linq.Expressions;

namespace Bloom.Application.Contracts.Ports;

public interface ISearchExerciseCatalogQuery
{
    public Task<IReadOnlyList<ExerciseData>> Fetch(
        Expression<Func<ExerciseData, bool>> filter
    );
}

public interface IFindWorkoutTemplatesQuery
{
    Task<IReadOnlyList<WorkoutTemplateData>> Fetch(
        Expression<Func<WorkoutTemplateData, bool>> filter
    );
}

public interface IFindLoggedWorkoutsQuery
{
    Task<IReadOnlyList<LoggedWorkoutData>> Fetch(
        Expression<Func<LoggedWorkoutData, bool>> filter
    );
}