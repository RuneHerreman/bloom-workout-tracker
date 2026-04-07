// using Bloom.Application.Contracts.Data.Templates;
// using Bloom.Domain.Entity;
// using Bloom.Domain.Templates;
//
// namespace Bloom.Application.Common.Mappings;
//
// public static class TemplateMappings
// {
//     public static WorkoutTemplateData ToDto(
//         this WorkoutTemplate template,
//         Dictionary<Guid, string> exerciseNames)
//         => new(
//             template.Id,
//             template.Name,
//             template.Exercises
//                 .OrderBy(e => e.Order)
//                 .Select(e => e.ToDto(exerciseNames))
//                 .ToList()
//         );
//
//     public static WorkoutTemplateExerciseData ToDto(
//         this WorkoutTemplateExercise exercise,
//         Dictionary<Guid, string> exerciseNames)
//         => new(
//             exercise.ExerciseId,
//             exerciseNames.GetValueOrDefault(exercise.ExerciseId, "Unknown"),
//             exercise.Order,
//             exercise.Sets
//                 .OrderBy(s => s.SetOrder)
//                 .Select(s => s.ToDto())
//                 .ToList()
//         );
//
//     public static TemplateExerciseSetData ToDto(this TemplateExerciseSet set)
//         => new(
//             set.SetOrder!.Value,
//             set.Reps!.Value,
//             set.RIR
//         );
//     
//     
// }
