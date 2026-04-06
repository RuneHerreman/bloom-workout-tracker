using Bloom.Domain.LogBook;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Repositories;

public class LogBookRepository : ILogBookRepository
{
    public Task<bool> Exists(LoggedWorkoutId id)
    {
        throw new NotImplementedException();
    }

    public Task<LoggedWorkout> ById(LoggedWorkoutId id)
    {
        throw new NotImplementedException();
    }

    public Task Save(LoggedWorkout aggregateRoot)
    {
        throw new NotImplementedException();
    }

    public Task Remove(LoggedWorkout aggregateRoot)
    {
        throw new NotImplementedException();
    }

    public Task<List<LoggedWorkout>> ByUserId(UserId userId)
    {
        throw new NotImplementedException();
    }
}