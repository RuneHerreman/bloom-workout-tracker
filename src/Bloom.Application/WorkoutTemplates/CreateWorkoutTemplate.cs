using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.WorkoutTemplates;

public sealed record CreateWorkoutTemplateInput(
    Guid UserId,
    string Name,
    List<TemplateExerciseInput> Exercises
);

public sealed record CreateWorkoutTemplateOutput(Guid WorkoutTemplateId);

public class CreateWorkoutTemplate(
    IUnitOfWork uow,
    ILogger<CreateWorkoutTemplate> logger
) : IUseCase<CreateWorkoutTemplateInput, CreateWorkoutTemplateOutput>
{
    public async Task<CreateWorkoutTemplateOutput> Execute(CreateWorkoutTemplateInput input)
    {
        logger.LogInformation($"Creating WorkoutTemplate | User: {input.UserId}");

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var userExists = await uow.Repo<IUserRepository>().Exists(EntityId.New<UserId>(input.UserId));

        if (!userExists)
            throw new UserNotFoundException($"User not found | Id: {input.UserId}");

        var exercises = input.Exercises.Select(e => e.ToTemplateExercise()).ToList();

        var template = WorkoutTemplate.Create(
            EntityId.New<UserId>(input.UserId),
            input.Name,
            exercises
        );

        await templateRepo.Save(template);//
        await uow.Do();

        logger.LogInformation($"WorkoutTemplate created | Id: {template.Id} - User: {input.UserId}");

        return new CreateWorkoutTemplateOutput(template.Id.Value);
    }
}