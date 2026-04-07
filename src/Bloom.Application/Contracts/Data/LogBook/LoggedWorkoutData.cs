namespace Bloom.Application.Contracts.Data.LogBook;

public record LoggedWorkoutData(
    Guid Id,
    string Name,
    DateTime Date,
    decimal Volume,
    List<LoggedExerciseData> Exercises    
);