namespace Bloom.Application.DTO.LogBook;

public record LoggedExerciseDTO(
    Guid ExerciseId,
    int Order,
    List<LoggedSetDTO> Sets
);