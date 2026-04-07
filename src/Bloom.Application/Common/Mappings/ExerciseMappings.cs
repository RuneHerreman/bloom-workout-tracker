// using Bloom.Application.Contracts.Data.Templates;
// using Bloom.Domain.Entity;
// using Bloom.Domain.Templates;
//
// namespace Bloom.Application.Common.Mappings;
//
// public static class ExerciseMappings
// {
//     public static List<WorkoutTemplateExercise> MapExercises(
//         List<WorkoutTemplateExerciseData> dtos,
//         Guid templateId)
//     {
//         var exercises = new List<WorkoutTemplateExercise>();
//
//         foreach (var dto in dtos)
//         {
//             var exercise = new WorkoutTemplateExercise
//             {
//                 Id = Guid.NewGuid(),
//                 WorkoutTemplateId = templateId,
//                 ExerciseId = dto.ExerciseId,
//                 Order = dto.Order
//             };
//
//             exercise.Sets = dto.Sets.Select(s => new TemplateExerciseSet
//                 {
//                     SetOrder = s.SetOrder,
//                     Reps = s.Reps,
//                     RIR = s.RIR,
//                     WorkoutTemplateExercise = exercise
//                 })
//                 .ToList();
//             exercises.Add(exercise);
//         }
//
//         return exercises;
//     }
// }