using System.Linq.Expressions;

namespace Bloom.Application.Contracts.Ports;

public interface ISearchExerciseCatalogQuery
{
    Task<IReadOnlyList<ExerciseData>> Fetch(
        Expression<Func<ExerciseData, bool>> filter,
        CancellationToken ct = default
    );
}

public interface IFindWorkoutTemplatesQuery
{
    Task<IReadOnlyList<WorkoutTemplateData>> Fetch(
        Expression<Func<WorkoutTemplateData, bool>> filter,
        CancellationToken ct = default
    );
}

public interface IFindLoggedWorkoutsQuery
{
    Task<IReadOnlyList<LoggedWorkoutData>> Fetch(
        Expression<Func<LoggedWorkoutData, bool>> filter,
        CancellationToken ct = default
    );
}

public interface IFindUsersQuery
{
    Task<IReadOnlyList<UserData>> Fetch(
        Expression<Func<UserData, bool>> filter,
        CancellationToken ct = default
    );
}