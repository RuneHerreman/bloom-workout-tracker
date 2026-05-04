using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct TemplateExerciseId(Guid Value) : IEntityId;

public class TemplateExercise: Entity<TemplateExerciseId>
{
    private readonly List<PlannedSet> _plannedSets = [];
    
    public ExerciseId ExerciseId { get; private set; }
    public IReadOnlyList<PlannedSet> PlannedSets => _plannedSets.AsReadOnly();
    
    private TemplateExercise() { }
    
    private TemplateExercise(
        TemplateExerciseId id, 
        ExerciseId exerciseId, 
        List<PlannedSet> plannedSets) : base(id)
    {
        ExerciseId = exerciseId;
        _plannedSets = plannedSets;
    }

    public static TemplateExercise Create(
        ExerciseId exerciseId, 
        List<PlannedSet> plannedSets, 
        TemplateExerciseId? templateExerciseId = null)
    {
        var templateExercise = new TemplateExercise(
            templateExerciseId ?? EntityId.New<TemplateExerciseId>(),
            exerciseId,
            plannedSets);
        
        templateExercise.ValidateState();
        return templateExercise;
    }
    
    
    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(ExerciseId);
        Asserts.EnsureNotEmpty(PlannedSets);
    }
}