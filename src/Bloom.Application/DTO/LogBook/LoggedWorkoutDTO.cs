namespace Bloom.Application.DTO.LogBook;

public record LoggedWorkoutDTO(
    Guid Id,
    DateTime Date,
    decimal Volume,
    List<LoggedExerciseDTO> Exercises    
);