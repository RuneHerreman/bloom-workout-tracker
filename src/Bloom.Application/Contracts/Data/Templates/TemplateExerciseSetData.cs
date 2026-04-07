namespace Bloom.Application.Contracts.Data.Templates;

public record TemplateExerciseSetData
(
    int SetOrder,
    int Reps,
    int? RIR = null
);