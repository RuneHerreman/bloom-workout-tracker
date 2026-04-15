using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Templates;

public readonly record struct WorkoutTemplateExerciseId(Guid Value) : IEntityId;

public class WorkoutTemplateExercise: AggregateRoot<WorkoutTemplateExerciseId>
{
    public WorkoutTemplateId WorkoutTemplateId { get; private set; }
    public ExerciseId ExerciseId { get; private set; }
    public int Order { get; private set; }
    public virtual List<TemplateStrengthSet> StrengthSets { get; private set; }
    public virtual List<TemplateCardioSet> CardioSets { get; private set; }

    // EF Core requires a parameterless constructor
    private WorkoutTemplateExercise() 
    {
        StrengthSets = new List<TemplateStrengthSet>();
        CardioSets = new List<TemplateCardioSet>();
    }

    private WorkoutTemplateExercise(WorkoutTemplateExerciseId id, WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId, int order) : base(id)
    {
        WorkoutTemplateId = workoutTemplateId;
        ExerciseId = exerciseId;
        Order = order;
        StrengthSets = new List<TemplateStrengthSet>();
        CardioSets = new List<TemplateCardioSet>();
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

    public void AddSet(TemplateStrengthSet set)
    {
        StrengthSets.Add(set);
        ValidateState();
    }

    public void AddSet(TemplateCardioSet set)
    {
        CardioSets.Add(set);
        ValidateState();
    }

    public override void ValidateState()
    {
        if (ExerciseId == default)
            throw new InvalidOperationException("ExerciseId must be set.");

        if (Order < 0)
            throw new InvalidOperationException("Order must be a non-negative integer.");

        if (StrengthSets.Count > 0 && CardioSets.Count > 0)
            throw new InvalidOperationException("An exercise cannot contain both strength and cardio sets.");
    }
}
