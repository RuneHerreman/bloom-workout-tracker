using Bloom.Application.Contracts.Data;
using Bloom.Application.Contracts.Data.LogBook;
using Bloom.Application.Contracts.Data.Templates;
using Bloom.Domain.Users;

namespace Bloom.Application.Contracts.Ports;

public interface IGetAllExercisesQuery
{
    public Task<List<ExerciseData>> Fetch();
}

public interface IGetAllUserLogsQuery
{
    public Task<List<LoggedWorkoutData>> Fetch(UserId userId);
}

public interface IGetAllUserTemplatesQuery
{
    public Task<List<WorkoutTemplateData>> Fetch(UserId userId);
}

public interface IGetTemplateByIdQuery
{
    public Task<WorkoutTemplateData?> Fetch(Guid templateId, UserId userId);
}