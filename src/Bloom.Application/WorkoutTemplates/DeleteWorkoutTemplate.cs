using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.WorkoutTemplates;

public sealed record DeleteWorkoutTemplateInput(Guid TemplateId, Guid UserId);

public class DeleteWorkoutTemplate(
    IUnitOfWork uow,
    ILogger<DeleteWorkoutTemplate> logger
) : IUseCase<DeleteWorkoutTemplateInput>
{
    public async Task Execute(DeleteWorkoutTemplateInput input)
    {
        logger.LogInformation($"Deleting WorkoutTemplate | Id: {input.TemplateId} - User: {input.UserId}");

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var template = await templateRepo.ById(EntityId.New<WorkoutTemplateId>(input.TemplateId));

        if (!template.HasValue)
            throw new WorkoutTemplateNotFoundException($"Template not found | Id: {input.TemplateId}");

        if (template.Value.UserId.Value != input.UserId)
            throw new WorkoutTemplateAccessDeniedException($"User {input.UserId} does not own template {input.TemplateId}");

        await templateRepo.Remove(template.Value);
        await uow.Do();

        logger.LogInformation($"WorkoutTemplate deleted | Id: {input.TemplateId} - User: {input.UserId}");
    }
}