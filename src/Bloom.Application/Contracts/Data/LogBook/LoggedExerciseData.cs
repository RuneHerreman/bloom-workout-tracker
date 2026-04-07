namespace Bloom.Application.Contracts.Data.LogBook;

public record LoggedExerciseData(
    Guid ExerciseId,
    int Order,
    List<LoggedSetData> Sets
);