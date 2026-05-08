using Bloom.Application.Contracts;
using Bloom.Application.Exercises;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.Exercises;

public sealed class FindExerciseByIdTests
{
    [Fact]
    public async Task Execute_WithExistingId_ShouldReturnExercise()
    {
        Guid id = Guid.NewGuid();
        var data = new List<ExerciseData>
        {
            new() { Id = id, Name = "Bench Press", Description = "Compound", Type = "Strength" }
        };
        var useCase = new FindExerciseById(new MockSearchExerciseCatalogQuery(data));

        var result = await useCase.Execute(new FindExerciseByIdInput(id));

        Assert.Equal(id, result.Exercise.Id);
        Assert.Equal("Bench Press", result.Exercise.Name);
    }

    [Fact]
    public async Task Execute_WithMissingId_ShouldThrow()
    {
        var useCase = new FindExerciseById(new MockSearchExerciseCatalogQuery([]));

        await Assert.ThrowsAsync<ExerciseNotFoundException>(
            () => useCase.Execute(new FindExerciseByIdInput(Guid.NewGuid())));
    }

    [Fact]
    public async Task Execute_WithEmptyId_ShouldThrow()
    {
        var useCase = new FindExerciseById(new MockSearchExerciseCatalogQuery([
            new ExerciseData { Id = Guid.NewGuid(), Name = "x", Description = "y", Type = "Strength" }
        ]));

        await Assert.ThrowsAsync<ExerciseNotFoundException>(
            () => useCase.Execute(new FindExerciseByIdInput(Guid.Empty)));
    }
}
