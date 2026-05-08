using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;

namespace UnitTests.Application.Contracts.Data.Filters;

public sealed class ExerciseDataFiltersTests
{
    private static IReadOnlyList<ExerciseData> Sample()
    {
        return
        [
            new ExerciseData
            {
                Id = Guid.NewGuid(),
                Name = "Bench Press",
                Description = "Compound chest movement.",
                Type = "Strength",
                TargetMuscles = [new TargetMuscleData("Chest"), new TargetMuscleData("Triceps")]
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
    public void ByProperty_NoFilters_ShouldReturnAll()
    {
        var filter = ExerciseDataFilters.ByProperty(null, null, null);

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ByProperty_NameFilter_ShouldFilterByName()
    {
        var filter = ExerciseDataFilters.ByProperty("bench", null, null);

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Bench Press", result[0].Name);
    }

    [Fact]
    public void ByProperty_MuscleGroupFilter_ShouldFilterByMuscle()
    {
        var filter = ExerciseDataFilters.ByProperty(null, [new TargetMuscleData("Legs")], null);

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Sprint", result[0].Name);
    }

    [Fact]
    public void ByProperty_TypeFilter_ShouldFilterByType()
    {
        var filter = ExerciseDataFilters.ByProperty(null, null, [ExerciseType.Cardio]);

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Cardio", result[0].Type);
    }

    [Fact]
    public void ByProperty_WhitespaceName_ShouldBeIgnored()
    {
        var filter = ExerciseDataFilters.ByProperty("   ", null, null);

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ByProperty_EmptyMuscleGroupList_ShouldBeIgnored()
    {
        var filter = ExerciseDataFilters.ByProperty(null, [], null);

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ByProperty_EmptyTypeList_ShouldBeIgnored()
    {
        var filter = ExerciseDataFilters.ByProperty(null, null, []);

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ById_WithValidId_ShouldFilterById()
    {
        var sample = Sample();
        var target = sample[0];

        var filter = ExerciseDataFilters.ById(EntityId.New<ExerciseId>(target.Id));

        var result = sample.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal(target.Id, result[0].Id);
    }

    [Fact]
    public void ById_WithEmptyGuid_ShouldReturnNothing()
    {
        var filter = ExerciseDataFilters.ById(EntityId.New<ExerciseId>(Guid.Empty));

        var result = Sample().AsQueryable().Where(filter).ToList();

        Assert.Empty(result);
    }
}
