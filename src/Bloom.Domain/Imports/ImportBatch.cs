using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Imports;

public readonly record struct ImportBatchId(Guid Value) : IEntityId;

/// <summary>
/// One import: which file, when, and what it added (workouts, new custom
/// exercises, templates), so it can be undone later from the import history.
/// </summary>
public class ImportBatch : AggregateRoot<ImportBatchId>
{
    public const int MaxFileNameLength = 200;

    public UserId UserId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public DateTime ImportedAt { get; private set; }
    public List<Guid> WorkoutIds { get; private set; } = [];
    public List<Guid> ExerciseIds { get; private set; } = [];
    public List<Guid> TemplateIds { get; private set; } = [];
    public List<Guid> BodyMeasurementIds { get; private set; } = [];
    public DateTime? UndoneAt { get; private set; }

    public bool IsUndone => UndoneAt is not null;

    private ImportBatch() { }

    private ImportBatch(ImportBatchId id, UserId userId, string fileName, IEnumerable<Guid> workoutIds, IEnumerable<Guid> exerciseIds, IEnumerable<Guid> templateIds, IEnumerable<Guid> bodyMeasurementIds)
        : base(id)
    {
        UserId = userId;
        FileName = fileName;
        ImportedAt = DateTime.UtcNow;
        WorkoutIds = workoutIds.Distinct().ToList();
        ExerciseIds = exerciseIds.Distinct().ToList();
        TemplateIds = templateIds.Distinct().ToList();
        BodyMeasurementIds = bodyMeasurementIds.Distinct().ToList();
    }

    public static ImportBatch Create(UserId userId, string? fileName, IEnumerable<Guid> workoutIds, IEnumerable<Guid> exerciseIds, IEnumerable<Guid> templateIds, IEnumerable<Guid>? bodyMeasurementIds = null)
    {
        var name = string.IsNullOrWhiteSpace(fileName) ? "Import" : fileName.Trim();
        if (name.Length > MaxFileNameLength) name = name[..MaxFileNameLength];

        var batch = new ImportBatch(EntityId.New<ImportBatchId>(), userId, name, workoutIds, exerciseIds, templateIds, bodyMeasurementIds ?? []);
        batch.ValidateState();
        return batch;
    }

    public void MarkUndone()
    {
        if (IsUndone) throw new InvalidOperationException("This import was already undone.");
        UndoneAt = DateTime.UtcNow;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(UserId);
        Asserts.EnsureNotEmpty(FileName);
    }
}
