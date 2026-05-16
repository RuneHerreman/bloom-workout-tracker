using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.WorkoutTemplates;

public sealed record CreateWorkoutTemplateInput(
    string Name,
    List<TemplateExerciseInput> Exercises
);

public sealed record CreateWorkoutTemplateOutput(Guid WorkoutTemplateId);

public class CreateWorkoutTemplate(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<CreateWorkoutTemplate> logger
) : IUseCase<CreateWorkoutTemplateInput, CreateWorkoutTemplateOutput>
{
    public async Task<CreateWorkoutTemplateOutput> Execute(CreateWorkoutTemplateInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Creating WorkoutTemplate | User: {UserId}", userId);

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var userExists = await uow.Repo<IUserRepository>().Exists(userId);

        if (!userExists)
            throw new UserNotFoundException($"User not found | Id: {userId}");

        var exercises = input.Exercises.Select(e => e.ToTemplateExercise()).ToList();

        var template = WorkoutTemplate.Create(
            userId,
            input.Name,
            exercises
        );

        await templateRepo.Save(template);
        await uow.Do(ct);

        logger.LogInformation("WorkoutTemplate created | Id: {Id} - User: {UserId}", template.Id, userId);

        return new CreateWorkoutTemplateOutput(template.Id.Value);
    }
}
