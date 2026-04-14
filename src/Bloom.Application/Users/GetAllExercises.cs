using Bloom.Application.Common;
using Bloom.Application.Contracts.Data;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Users;

public sealed record GetAlLExercisesInput(
    Guid userId
);

public class GetAllExercises(
    IUnitOfWork unitOfWork,
    IGetAllExercisesQuery getAllExercisesQuery
): IUseCase<GetAlLExercisesInput,IEnumerable<ExerciseData>>
{
    public async Task<IEnumerable<ExerciseData>> Execute(GetAlLExercisesInput input)
    {
        var exists = await unitOfWork.Repo<IUserRepository>().Exists(new UserId(input.userId));;
        
        if (!exists)
            throw new UserDoesNotExistError($"This user is not valid.");
        
        return await getAllExercisesQuery.Fetch();
    }
}