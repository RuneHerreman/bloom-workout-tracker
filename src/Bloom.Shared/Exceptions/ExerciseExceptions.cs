namespace Bloom.Shared.Exceptions;


public class ExerciseNotFoundException(Guid id): Exception($"Exercise not found | {id} ") {}

public class ExerciseAlreadyExistsException(string name): Exception($"Exercise already exists | Name: {name}") {}

public class ExerciseAccessDeniedException(string message): Exception(message) {}
