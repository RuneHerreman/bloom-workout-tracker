using System.ComponentModel.DataAnnotations;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record CustomExerciseBody(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [MaxLength(2000)]
    string Description,

    [Required]
    [RegularExpression("^(Strength|Cardio|Plyometric)$", ErrorMessage = "Type must be Strength, Cardio, or Plyometric")]
    string Type,

    [Required]
    [MinLength(1, ErrorMessage = "At least one target muscle is required")]
    // Areas ("Back") or muscles in them ("Lats"); names outside the tree become their own area.
    List<string> TargetMuscles
);
