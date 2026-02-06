using Bloom.Application.Commands;
using Bloom.Application.DTO.Templates;
using Bloom.Domain.Entity;

namespace Bloom.Application.Common.Mappings;

public static class TemplateMappings
{
    public static WorkoutTemplateDTO ToDto(this WorkoutTemplate template)
        => new(
            template.Id,
            template.Name,
            template.Exercises
                .OrderBy(e => e.Order)
                .Select(e => e.ToDto())
                .ToList()
        );

    public static WorkoutTemplateExerciseDTO ToDto(this WorkoutTemplateExercise exercise)
        => new(
            exercise.ExerciseId,
            exercise.Order,
            exercise.Sets
                .OrderBy(s => s.SetOrder)
                .Select(s => s.ToDto())
                .ToList()
        );

    public static TemplateExerciseSetDTO ToDto(this TemplateExerciseSet set)
        => new(
            set.SetOrder!.Value,
            set.Reps!.Value,
            set.RIR
        );
    
    
}
