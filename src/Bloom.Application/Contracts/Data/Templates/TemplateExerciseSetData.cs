namespace Bloom.Application.Contracts.Data.Templates;

public record TemplateExerciseSetData
(
    int SetOrder,
    int? Reps = null,
    int? RIR = null,
    TimeOnly? Duration = null,
    decimal? Distance = null
);