namespace Bloom.Shared.Exceptions;

public sealed class UserAlreadyExistsError(string message): Exception(message) { }
public sealed class UserDoesNotExistError(string message): Exception(message) { }