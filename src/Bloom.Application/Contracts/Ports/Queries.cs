using Bloom.Application.DTO;
using Bloom.Application.DTO.LogBook;

namespace Bloom.Application.Contracts.Ports;

public interface IGetAllExercisesQuery
{
    public Task<List<ExerciseDTO>> Fetch();
}

public interface IGetAllUserLogsQuery
{
    public Task<List<LoggedWorkoutDTO>> Fetch(Guid userId);
}