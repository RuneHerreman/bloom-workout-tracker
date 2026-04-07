// using Bloom.Application.DTO;
// using Bloom.Domain.Entity;
// using Bloom.Domain.Exercises;
//
// namespace Bloom.Application.Common.Mappings;
//
// public static class GenericMappings
// {
//     public static ExerciseDTO ToDto(this Exercise exercise)
//         => new(
//             exercise.Id,
//             exercise.Name,
//             exercise.Description,
//             exercise.Type.ToString(),
//             exercise.PrimaryMuscleGroup
//         );
// }