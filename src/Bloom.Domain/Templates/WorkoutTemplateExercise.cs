using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Templates;

public readonly record struct WorkoutTemplateExerciseId(Guid Value) : IEntityId;

public class WorkoutTemplateExercise: AggregateRoot<WorkoutTemplateExerciseId>
{
    public WorkoutTemplateId WorkoutTemplateId { get; private set; }
    public ExerciseId ExerciseId { get; private set; }
    public int Order { get; private set; }
    public virtual List<ExerciseSet> Sets { get; private set; }

    // EF Core requires a parameterless constructor
    private WorkoutTemplateExercise() 
    {
        Sets = new List<ExerciseSet>();
    }

    private WorkoutTemplateExercise(WorkoutTemplateExerciseId id, WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId, int order) : base(id)
    {
        WorkoutTemplateId = workoutTemplateId;
        ExerciseId = exerciseId;
        Order = order;
        Sets = new List<ExerciseSet>();
    }

    public static WorkoutTemplateExercise Create(WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId, int order, WorkoutTemplateExerciseId? id = null)
    {
        WorkoutTemplateExercise exercise = new(
            id ?? EntityId.New<WorkoutTemplateExerciseId>(),
            workoutTemplateId,
            exerciseId,
            order
        );
        exercise.ValidateState();
        return exercise;
    }

    public void AddSet(ExerciseSet set)
    {
        if (set is not TemplateStrengthSet && set is not TemplateCardioSet)
            throw new ArgumentException("Only template sets can be added to a template exercise.", nameof(set));

        Sets.Add(set);
    }

    public override void ValidateState()
    {
        if (ExerciseId == default)
            throw new InvalidOperationException("ExerciseId must be set.");

        if (Order < 0)
            throw new InvalidOperationException("Order must be a non-negative integer.");
    }
}
