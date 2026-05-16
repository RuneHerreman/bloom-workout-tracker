using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;

namespace UnitTests.Application.Exercises;

public sealed class SearchExerciseCatalogTests
{
    private static List<ExerciseData> Sample()
    {
        return
        [
            new ExerciseData
            {
                Id = Guid.NewGuid(),
                Name = "Bench Press",
                Description = "Compound chest movement.",
                Type = "Strength",
                TargetMuscles = [new TargetMuscleData("Chest")]
            },
            new ExerciseData
            {
                Id = Guid.NewGuid(),
                Name = "Sprint",
                Description = "Cardio sprint.",
                Type = "Cardio",
                TargetMuscles = [new TargetMuscleData("Legs")]
            }
        ];
    }

    [Fact]
    public async Task Execute_WithNoFilters_ShouldReturnAll()
    {
        var useCase = new SearchExerciseCatalog(new MockSearchExerciseCatalogQuery(Sample()));

        var result = await useCase.Execute(new SearchExerciseCatalogInput(null, null, null));

        Assert.Equal(2, result.Exercises.Count);
    }

    [Fact]
    public async Task Execute_WithName_ShouldFilter()
    {
        var useCase = new SearchExerciseCatalog(new MockSearchExerciseCatalogQuery(Sample()));

        var result = await useCase.Execute(new SearchExerciseCatalogInput("bench", null, null));

        Assert.Single(result.Exercises);
    }

    [Fact]
    public async Task Execute_WithMuscleGroups_ShouldFilter()
    {
        var useCase = new SearchExerciseCatalog(new MockSearchExerciseCatalogQuery(Sample()));

        var result = await useCase.Execute(new SearchExerciseCatalogInput(null, ["Legs"], null));

        Assert.Single(result.Exercises);
        Assert.Equal("Sprint", result.Exercises[0].Name);
    }

    [Fact]
    public async Task Execute_WithExerciseTypes_ShouldFilter()
    {
        var useCase = new SearchExerciseCatalog(new MockSearchExerciseCatalogQuery(Sample()));

        var result = await useCase.Execute(new SearchExerciseCatalogInput(null, null, ["Strength"]));

        Assert.Single(result.Exercises);
        Assert.Equal("Bench Press", result.Exercises[0].Name);
    }

    [Fact]
    public async Task Execute_WithUnknownType_ShouldIgnore()
    {
        var useCase = new SearchExerciseCatalog(new MockSearchExerciseCatalogQuery(Sample()));

        var result = await useCase.Execute(new SearchExerciseCatalogInput(null, null, ["NotARealType"]));

        Assert.Equal(2, result.Exercises.Count);
    }

    [Fact]
    public async Task Execute_WithEmptyTypeList_ShouldBeIgnored()
    {
        var useCase = new SearchExerciseCatalog(new MockSearchExerciseCatalogQuery(Sample()));

        var result = await useCase.Execute(new SearchExerciseCatalogInput(null, null, []));

        Assert.Equal(2, result.Exercises.Count);
    }
}

public sealed class MockSearchExerciseCatalogQuery(IEnumerable<ExerciseData> data) : ISearchExerciseCatalogQuery
{
    private readonly List<ExerciseData> _data = data.ToList();

    public Task<IReadOnlyList<ExerciseData>> Fetch(Expression<Func<ExerciseData, bool>> filter, CancellationToken ct = default)
    {
        IReadOnlyList<ExerciseData> filtered = _data.AsQueryable().Where(filter).ToList();
        return Task.FromResult(filtered);
    }
}
