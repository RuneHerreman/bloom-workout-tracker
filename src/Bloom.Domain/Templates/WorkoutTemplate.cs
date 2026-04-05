using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Templates;

public readonly record struct WorkoutTemplateId(Guid Value) : IEntityId;

public class WorkoutTemplate: AggregateRoot<WorkoutTemplateId>
{
    public UserId UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public virtual List<WorkoutTemplateExercise> Exercises { get; private set; }

    // EF Core requires a parameterless constructor
    private WorkoutTemplate() 
    {
        Exercises = new List<WorkoutTemplateExercise>();
    }

    private WorkoutTemplate(WorkoutTemplateId id, UserId userId, string name) : base(id)
    {
        UserId = userId;
        Name = name;
        Exercises = new List<WorkoutTemplateExercise>();
    }

    public static WorkoutTemplate Create(UserId userId, string name, WorkoutTemplateId? templateId = null)
    {
        WorkoutTemplate template = new(
            templateId ?? EntityId.New<WorkoutTemplateId>(),
            userId,
            name
        );
        template.ValidateState();
        return template;
    }

    public void AddExercise(WorkoutTemplateExercise exercise)
    {
        Exercises.Add(exercise);
    }

    public void UpdateName(string name)
    {
        Name = name;
        ValidateState();
    }

    public override void ValidateState()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Workout template name cannot be empty.");

        if (UserId == default)
            throw new InvalidOperationException("UserId must be set.");
    }
}