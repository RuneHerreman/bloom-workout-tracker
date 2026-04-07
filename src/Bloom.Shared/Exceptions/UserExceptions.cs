namespace Bloom.Shared.Exceptions;

public sealed class UserAlreadyExistsError(string message): Exception(message)
{
    
}