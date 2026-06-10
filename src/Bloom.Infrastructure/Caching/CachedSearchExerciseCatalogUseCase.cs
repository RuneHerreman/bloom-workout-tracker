using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Microsoft.Extensions.Caching.Memory;

namespace Bloom.Infrastructure.Caching;

public sealed class CachedSearchExerciseCatalogUseCase(
    IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput> inner,
    IMemoryCache cache,
    ICurrentUser currentUser,
    ExerciseCatalogCacheVersion cacheVersion
) : IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public Task<SearchExerciseCatalogOutput> Execute(SearchExerciseCatalogInput input, CancellationToken ct = default)
    {
        // Results are scoped per user (custom exercises), so the key must be too.
        // The version token invalidates a user's entries when their custom exercises change.
        var userId = currentUser.UserId.Value;
        var cacheKey = $"{userId}:v{cacheVersion.Get(userId)}:{BuildCacheKey(input)}";
        return cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return inner.Execute(input, ct);
        })!;
    }

    private static string BuildCacheKey(SearchExerciseCatalogInput input)
    {
        var name = input.Name?.Trim().ToLowerInvariant() ?? string.Empty;
        var muscleGroups = input.TargetMuscleGroups is { Count: > 0 }
            ? string.Join(',', input.TargetMuscleGroups.Select(g => g.ToLowerInvariant()).Order())
            : string.Empty;
        var types = input.ExerciseTypes is { Count: > 0 }
            ? string.Join(',', input.ExerciseTypes.Select(t => t.ToLowerInvariant()).Order())
            : string.Empty;

        return $"exercises:{name}|{muscleGroups}|{types}";
    }
}
