using Bloom.Application.DTO.LogBook;
using Bloom.Domain.Entity.Logs;

namespace Bloom.Application.Common.Mappings;

public static class LogBookMappings
{
    public static LoggedWorkoutDTO ToDtoShort(this LoggedWorkout loggedWorkout)
        => new(
            loggedWorkout.Id,
            loggedWorkout.Name,
            loggedWorkout.Date,
            loggedWorkout.Volume,
            new List<LoggedExerciseDTO>()
        );
}