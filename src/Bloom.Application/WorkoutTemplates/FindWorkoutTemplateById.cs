using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.WorkoutTemplates;

public sealed record FindWorkoutTemplateByIdInput(Guid TemplateId);

public sealed record FindWorkoutTemplateByIdOutput(WorkoutTemplateData Template);

public class FindWorkoutTemplateById(
    ICurrentUser currentUser,
    IFindWorkoutTemplatesQuery query
) : IUseCase<FindWorkoutTemplateByIdInput, FindWorkoutTemplateByIdOutput>
{
    public async Task<FindWorkoutTemplateByIdOutput> Execute(FindWorkoutTemplateByIdInput input, CancellationToken ct = default)
    {
        var templates = await query.Fetch(
            WorkoutTemplateDataFilters.ById(EntityId.New<WorkoutTemplateId>(input.TemplateId)), ct
        );

        var result = templates.FirstOrDefault()
            ?? throw new WorkoutTemplateNotFoundException($"Template not found | Id: {input.TemplateId}");

        if (result.UserId != currentUser.UserId.Value)
            throw new WorkoutTemplateAccessDeniedException($"User {currentUser.UserId.Value} does not own template {input.TemplateId}");

        return new FindWorkoutTemplateByIdOutput(result);
    }
}