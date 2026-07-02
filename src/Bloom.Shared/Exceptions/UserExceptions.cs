namespace Bloom.Shared.Exceptions;

public sealed class UserAlreadyExistsException(string message): Exception(message) { }
public sealed class UserNotFoundException(string message): Exception(message) { }
public sealed class UserAccessDeniedException(string message): Exception(message) { }
public sealed class InvalidCredentialsException(string message): Exception(message) { }
public sealed class UnauthenticatedException(string message): Exception(message) { }