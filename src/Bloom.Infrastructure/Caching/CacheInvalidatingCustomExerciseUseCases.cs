using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;

namespace Bloom.Infrastructure.Caching;

public sealed class CacheInvalidatingCreateCustomExerciseUseCase(
    IUseCase<CreateCustomExerciseInput, CreateCustomExerciseOutput> inner,
    ExerciseCatalogCacheVersion cacheVersion,
    ICurrentUser currentUser
) : IUseCase<CreateCustomExerciseInput, CreateCustomExerciseOutput>
{
    public async Task<CreateCustomExerciseOutput> Execute(CreateCustomExerciseInput input, CancellationToken ct = default)
    {
        var output = await inner.Execute(input, ct);
        cacheVersion.Bump(currentUser.UserId.Value);
        return output;
    }
}

public sealed class CacheInvalidatingUpdateCustomExerciseUseCase(
    IUseCase<UpdateCustomExerciseInput, UpdateCustomExerciseOutput> inner,
    ExerciseCatalogCacheVersion cacheVersion,
    ICurrentUser currentUser
) : IUseCase<UpdateCustomExerciseInput, UpdateCustomExerciseOutput>
{
    public async Task<UpdateCustomExerciseOutput> Execute(UpdateCustomExerciseInput input, CancellationToken ct = default)
    {
        var output = await inner.Execute(input, ct);
        cacheVersion.Bump(currentUser.UserId.Value);
        return output;
    }
}

public sealed class CacheInvalidatingDeleteCustomExerciseUseCase(
    IUseCase<DeleteCustomExerciseInput> inner,
    ExerciseCatalogCacheVersion cacheVersion,
    ICurrentUser currentUser
) : IUseCase<DeleteCustomExerciseInput>
{
    public async Task Execute(DeleteCustomExerciseInput input, CancellationToken ct = default)
    {
        await inner.Execute(input, ct);
        cacheVersion.Bump(currentUser.UserId.Value);
    }
}
