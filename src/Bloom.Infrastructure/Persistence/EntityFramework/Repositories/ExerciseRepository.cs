using Aornis;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class ExerciseRepository(DomainDbContext context) : EfCoreGenericRepository<Exercise, ExerciseId>(context), IExerciseRepository
{
    public async Task<Optional<Exercise>> ByName(string name, CancellationToken ct = default)
    {
        var exerciseName = ExerciseName.Create(name);
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Name == exerciseName, ct);
        return Optional.Of(exercise);
    }

    public async Task<Optional<Exercise>> ByNameForUser(string name, UserId ownerUserId, CancellationToken ct = default)
    {
        var exerciseName = ExerciseName.Create(name);
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(
                e => e.Name == exerciseName && (e.OwnerUserId == null || e.OwnerUserId == ownerUserId),
                ct);
        return Optional.Of(exercise);
    }
}
