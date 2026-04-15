using Bloom.Application.Contracts.Data.Templates;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Templates;

public sealed record GetAllUserTemplatesInput(
    Guid UserId
);

public class GetAllUserTemplates(
    IUnitOfWork unitOfWork,
    IGetAllUserTemplatesQuery getAllExercisesQuery
): IUseCase<GetAllUserTemplatesInput, IReadOnlyList<WorkoutTemplateData>>
{
    public async Task<IReadOnlyList<WorkoutTemplateData>> Execute(GetAllUserTemplatesInput input)
    {
        var exists = await unitOfWork.Repo<IUserRepository>().Exists(new UserId(input.UserId));
        
        if (!exists)
            throw new UserDoesNotExistError($"This user is not valid.");

        return await getAllExercisesQuery.Fetch(new UserId(input.UserId));
    }
}