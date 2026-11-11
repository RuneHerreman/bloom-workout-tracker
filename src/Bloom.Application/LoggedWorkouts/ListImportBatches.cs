using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Imports;

namespace Bloom.Application.LoggedWorkouts;

public sealed record ListImportBatchesInput;

public sealed record ImportBatchSummary(
    Guid Id,
    string FileName,
    DateTime ImportedAt,
    int Workouts,
    int Exercises,
    int Templates,
    DateTime? UndoneAt
);

public sealed record ListImportBatchesOutput(IReadOnlyList<ImportBatchSummary> Imports);

/// <summary>Your import history, newest first, for the import page.</summary>
public class ListImportBatches(IUnitOfWork uow, ICurrentUser currentUser)
    : IUseCase<ListImportBatchesInput, ListImportBatchesOutput>
{
    public async Task<ListImportBatchesOutput> Execute(ListImportBatchesInput input, CancellationToken ct = default)
    {
        var batches = await uow.Repo<IImportBatchRepository>().ForUser(currentUser.UserId, ct);
        return new ListImportBatchesOutput(batches
            .Select(b => new ImportBatchSummary(b.Id.Value, b.FileName, b.ImportedAt, b.WorkoutIds.Count, b.ExerciseIds.Count, b.TemplateIds.Count, b.UndoneAt))
            .ToList());
    }
}
