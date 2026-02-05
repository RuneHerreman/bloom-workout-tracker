namespace Bloom.Application.DTO.LogBook;

public record LoggedSetDTO(
    int? SetOrder,
    int? Reps,
    int? Weight,
    int? RIR,
    TimeOnly? Duration,
    decimal? Distance
);