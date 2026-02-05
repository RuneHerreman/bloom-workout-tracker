namespace Bloom.Domain.Entity;

public enum ExerciseType
{
    Cardio,
    Strength,
    Plyometric
}
public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ExerciseType Type { get; set; }
    public string PrimaryMuscleGroup { get; set; } = null!;
    
    private Exercise() {}
    
    public Exercise(Guid id, string name, string description, ExerciseType type, string primaryMuscleGroup)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        PrimaryMuscleGroup = primaryMuscleGroup;
    }
}