using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct TemplateExerciseId(Guid Value) : IEntityId;

public class TemplateExercise : Entity<TemplateExerciseId>
{
    private readonly List<PlannedSet> _sets = [];

    public ExerciseId ExerciseId { get; private set; }
    public int Order { get; private set; }
    public string? Note { get; private set; }
    public List<string> Gear { get; private set; } = [];
    public IReadOnlyList<PlannedSet> Sets => _sets.AsReadOnly();

    private TemplateExercise() { }

    private TemplateExercise(
        TemplateExerciseId id,
        ExerciseId exerciseId,
        int order,
        string? note,
        List<string> gear,
        List<PlannedSet> sets) : base(id)
    {
        ExerciseId = exerciseId;
        Order = order;
        Note = note;
        Gear = gear;
        _sets = sets;
    }

    public static TemplateExercise Create(
        ExerciseId exerciseId,
        int order,
        List<PlannedSet> sets,
        TemplateExerciseId? id = null,
        string? note = null,
        List<string>? gear = null)
    {
        var te = new TemplateExercise(id ?? EntityId.New<TemplateExerciseId>(), exerciseId, order, note, gear ?? [], sets);
        te.ValidateState();
        return te;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(ExerciseId);
        Asserts.EnsureNotNegative(Order);
        Asserts.EnsureNotEmpty(_sets);
    }
}