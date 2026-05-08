using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.WorkoutTemplates;

public sealed record DeleteWorkoutTemplateInput(Guid TemplateId);

public class DeleteWorkoutTemplate(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<DeleteWorkoutTemplate> logger
) : IUseCase<DeleteWorkoutTemplateInput>
{
    public async Task Execute(DeleteWorkoutTemplateInput input)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Deleting WorkoutTemplate | Id: {Id} - User: {UserId}", input.TemplateId, userId);

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var template = await templateRepo.ById(EntityId.New<WorkoutTemplateId>(input.TemplateId));

        if (!template.HasValue)
            throw new WorkoutTemplateNotFoundException($"Template not found | Id: {input.TemplateId}");

        if (template.Value.UserId != userId)
            throw new WorkoutTemplateAccessDeniedException($"User {userId} does not own template {input.TemplateId}");

        await templateRepo.Remove(template.Value);
        await uow.Do();

        logger.LogInformation("WorkoutTemplate deleted | Id: {Id} - User: {UserId}", input.TemplateId, userId);
    }
}
