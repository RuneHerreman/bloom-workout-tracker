using Bloom.Application.Contracts;

namespace Bloom.Infrastructure.WebApi.Controllers.Responses;

public record Exercise (
     Guid Id,
     string Name,
     string Description,
     string Type,
     bool IsCustom,
     IReadOnlyList<string> TargetMuscles,
     IReadOnlyList<string> SecondaryMuscles
);

public static class ExerciseExtensions {
    public static Exercise ToResponse(this ExerciseData data)
    {
        return new Exercise(
            data.Id,
            data.Name,
            data.Description,
            data.Type,
            data.OwnerUserId is not null,
            data.TargetMuscles.Select(mg => mg.Value).ToList(),
            data.SecondaryMuscles.Select(mg => mg.Value).ToList()
        );
    }
}