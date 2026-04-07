// using Bloom.Application.Contracts.Data.LogBook;
// using Bloom.Domain.LogBook;
//
// namespace Bloom.Application.Common.Mappings;
//
// public static class LogBookMappings
// {
//     public static LoggedWorkoutData ToDtoShort(this LoggedWorkout loggedWorkout)
//         => new(
//             loggedWorkout.Id,
//             loggedWorkout.Name,
//             loggedWorkout.Date,
//             loggedWorkout.Volume,
//             new List<LoggedExerciseData>()
//         );
// }