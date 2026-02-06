using Bloom.Application.DTO.Templates;
using Bloom.Domain.Entity;

namespace Bloom.Application.Common.Mappings;

public static class ExerciseMappings
{
    public static List<WorkoutTemplateExercise> MapExercises(
        List<WorkoutTemplateExerciseDTO> dtos,
        Guid templateId)
    {
        var exercises = new List<WorkoutTemplateExercise>();

        foreach (var dto in dtos)
        {
            var exercise = new WorkoutTemplateExercise
            {
                Id = Guid.NewGuid(),
                WorkoutTemplateId = templateId,
                ExerciseId = dto.ExerciseId,
                Order = dto.Order
            };

            exercise.Sets = dto.Sets.Select(s => new TemplateExerciseSet
                {
                    SetOrder = s.SetOrder,
                    Reps = s.Reps,
                    RIR = s.RIR,
                    WorkoutTemplateExercise = exercise
                })
                .ToList();
            exercises.Add(exercise);
        }

        return exercises;
    }
}