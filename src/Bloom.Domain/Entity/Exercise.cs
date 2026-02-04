using Bloom.Domain.Entity.Enums;

namespace Bloom.Domain.Entity;

public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ExerciseType Type { get; set; }
}