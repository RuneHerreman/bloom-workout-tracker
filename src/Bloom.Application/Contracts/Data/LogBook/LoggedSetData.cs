namespace Bloom.Application.Contracts.Data.LogBook;

public record LoggedSetData(
    int? SetOrder,
    int? Reps,
    int? Weight,
    int? RIR,
    TimeOnly? Duration,
    decimal? Distance
);