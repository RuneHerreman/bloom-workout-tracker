using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct TemplateExerciseId(Guid Value) : IEntityId;

public class TemplateExercise: Entity<TemplateExerciseId>
{
    private readonly List<PlannedCardioSet> _plannedCardioSets = [];
    private readonly List<PlannedStrengthSet> _plannedStrengthSets = [];
    
    public ExerciseId ExerciseId { get; private set; }
    public IEnumerable<PlannedCardioSet> PlannedCardioSets => _plannedCardioSets.AsReadOnly();
    public IEnumerable<PlannedStrengthSet> PlannedStrengthSets => _plannedStrengthSets.AsReadOnly();
    public IEnumerable<PlannedSet> PlannedSets => [.._plannedCardioSets, .._plannedStrengthSets];
    private TemplateExercise() { }
    
    private TemplateExercise(
        TemplateExerciseId id, 
        ExerciseId exerciseId, 
        List<PlannedCardioSet> plannedCardioSets,
        List<PlannedStrengthSet> plannedStrengthSets) : base(id)
    {
        ExerciseId = exerciseId;
        _plannedCardioSets = plannedCardioSets;
        _plannedStrengthSets = plannedStrengthSets;
    }

    public static TemplateExercise Create(
        ExerciseId exerciseId, 
        List<PlannedCardioSet> plannedCardioSets,
        List<PlannedStrengthSet> plannedStrengthSets,
        TemplateExerciseId? templateExerciseId = null)
    {
        var templateExercise = new TemplateExercise(
            templateExerciseId ?? EntityId.New<TemplateExerciseId>(),
            exerciseId,
            plannedCardioSets,
            plannedStrengthSets);
        
        templateExercise.ValidateState();
        return templateExercise;
    }
    
    
    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(ExerciseId);
        Asserts.EnsureNotEmpty(PlannedSets);
    }
}