using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.WorkoutTemplates;

public sealed record UpdateWorkoutTemplateInput(
    Guid TemplateId,
    Guid UserId,
    string Name,
    List<TemplateExerciseInput> Exercises
);

public sealed record UpdateWorkoutTemplateOutput(Guid WorkoutTemplateId);

public class UpdateWorkoutTemplate(
    IUnitOfWork uow,
    ILogger<UpdateWorkoutTemplate> logger
) : IUseCase<UpdateWorkoutTemplateInput, UpdateWorkoutTemplateOutput>
{
    public async Task<UpdateWorkoutTemplateOutput> Execute(UpdateWorkoutTemplateInput input)
    {
        logger.LogInformation($"Updating WorkoutTemplate | Id: {input.TemplateId} - User: {input.UserId}");

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var template = await templateRepo.ById(EntityId.New<WorkoutTemplateId>(input.TemplateId));

        if (!template.HasValue)
            throw new WorkoutTemplateNotFoundException($"Template not found | Id: {input.TemplateId}");

        if (template.Value.UserId.Value != input.UserId)
            throw new WorkoutTemplateAccessDeniedException($"User {input.UserId} does not own template {input.TemplateId}");

        var exercises = input.Exercises.Select(e => e.ToTemplateExercise()).ToList();
        template.Value.Update(input.Name, exercises);

        await templateRepo.Save(template.Value);
        await uow.Do();

        logger.LogInformation($"WorkoutTemplate updated | Id: {template.Value.Id} - User: {input.UserId}");

        return new UpdateWorkoutTemplateOutput(template.Value.Id.Value);
    }
}