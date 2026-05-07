namespace Bloom.Shared.Exceptions;

public sealed class LoggedWorkoutNotFoundException(string message): Exception(message) { }
public sealed class LoggedWorkoutAccessDeniedException(string message): Exception(message) { }