using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises;

public readonly record struct ExerciseId(Guid Value) : IEntityId;

public class Exercise: AggregateRoot<ExerciseId>
{
    public ExerciseName Name { get; private set; }
    public ExerciseDescription Description { get; private set; }
    public ExerciseType Type { get; private set; }
    public MuscleGroup MuscleGroup { get; private set; }
    
    private Exercise() {}

    private Exercise(
        ExerciseId  id,  
        ExerciseName name,
        ExerciseDescription description,
        ExerciseType type,
        MuscleGroup muscleGroup
    ) : base(id)
    {
        Name = name;
        Description = description;
        Type = type;
        MuscleGroup = muscleGroup;
    }

    public static Exercise Create(
        string name,
        string description,
        ExerciseType type,
        MuscleGroup muscleGroup
    )
    {
        var exercise = new Exercise(
            EntityId.New<ExerciseId>(),
            ExerciseName.Create(name),
            ExerciseDescription.Create(description),
            type,
            muscleGroup
        );
        
        exercise.ValidateState();
        
        return exercise;;
    }
    
    
    public override void ValidateState()
    {
        
    }
}