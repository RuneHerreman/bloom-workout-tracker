namespace Bloom.Application.DTO.LogBook;

public record LoggedWorkoutDTO(
    Guid Id,
    string Name,
    DateTime Date,
    decimal Volume,
    List<LoggedExerciseDTO> Exercises    
);