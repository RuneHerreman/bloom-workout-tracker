namespace Bloom.Shared.Exceptions;

public sealed class InvalidWorkoutTemplateException(string message): Exception(message) { }
public sealed class WorkoutTemplateNotFoundException(string message): Exception(message) { }
public sealed class WorkoutTemplateAccessDeniedException(string message): Exception(message) { }
