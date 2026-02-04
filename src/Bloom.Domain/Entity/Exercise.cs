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
    
    private Exercise() {}
    
    public Exercise(Guid id, string name, string description, ExerciseType type)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
    }
}