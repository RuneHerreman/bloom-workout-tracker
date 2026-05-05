namespace Bloom.Shared.Exceptions;


public class ExerciseNotFoundException(Guid id): Exception($"Exercise not found | {id} ") {}