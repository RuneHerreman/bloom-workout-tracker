using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.LogBook;

public interface ILogBookRepository: IRepository<LoggedWorkout, LoggedWorkoutId>
{
    Task<List<LoggedWorkout>> ByUserId(UserId userId);
}