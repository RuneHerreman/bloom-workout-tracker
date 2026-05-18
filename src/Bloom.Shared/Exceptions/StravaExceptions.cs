namespace Bloom.Shared.Exceptions;

public sealed class StravaConnectionNotFoundException(string message) : Exception(message) { }
public sealed class StravaConnectionAlreadyExistsException(string message) : Exception(message) { }
