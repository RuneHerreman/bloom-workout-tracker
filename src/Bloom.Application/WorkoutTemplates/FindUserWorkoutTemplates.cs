using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;

namespace Bloom.Application.WorkoutTemplates;

public sealed record FindUserWorkoutTemplatesInput(string? Name);

public sealed record FindUserWorkoutTemplatesOutput(IReadOnlyList<WorkoutTemplateData> Templates);

public class FindUserWorkoutTemplates(
    ICurrentUser currentUser,
    IFindWorkoutTemplatesQuery query
) : IUseCase<FindUserWorkoutTemplatesInput, FindUserWorkoutTemplatesOutput>
{
    public async Task<FindUserWorkoutTemplatesOutput> Execute(FindUserWorkoutTemplatesInput input, CancellationToken ct = default)
    {
        var templates = await query.Fetch(
            WorkoutTemplateDataFilters.ByProperty(currentUser.UserId.Value, input.Name), ct
        );

        return new FindUserWorkoutTemplatesOutput(templates);
    }
}
