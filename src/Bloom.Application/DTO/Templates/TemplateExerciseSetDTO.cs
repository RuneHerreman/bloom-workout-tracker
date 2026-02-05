namespace Bloom.Application.DTO.Templates;

public record TemplateExerciseSetDTO
(
    int SetOrder,
    int Reps,
    int? RIR = null
);