namespace Bloom.Domain.Entity;

public abstract class ExerciseSet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Strength
    public int? SetOrder { get; set; }
    public int? Reps { get; set; }
    public int? RIR { get; set; }
    
    // Cardio
    public TimeOnly? Duration { get; set; }
    public decimal? Distance { get; set; }
    
    public bool IsStrength() => SetOrder.HasValue && Reps.HasValue;
    public bool IsCardio() => Duration.HasValue || Distance.HasValue;
}