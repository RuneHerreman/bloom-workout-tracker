using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Domain.WorkoutTemplates.DomainEvents;

namespace UnitTests.Domain.WorkoutTemplates;

public sealed class WorkoutTemplateTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeAndRaiseEvent()
    {
        UserId userId = EntityId.New<UserId>();
        List<TemplateExercise> exercises =
        [
            TemplateExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 8)])
        ];

        WorkoutTemplate template = WorkoutTemplate.Create(userId, "Leg Day", exercises);

        Assert.Equal(userId, template.UserId);
        Assert.Equal("Leg Day", template.Name.Value);
        Assert.Single(template.TemplateExercises);
        Assert.Single(template.DomainEvents);
        Assert.IsType<WorkoutTemplateCreated>(template.DomainEvents.First());
    }

    [Fact]
    public void Update_WithValidInput_ShouldReplaceStateAndRaiseEvent()
    {
        UserId userId = EntityId.New<UserId>();
        WorkoutTemplate template = WorkoutTemplate.Create(userId, "Push Day",
        [
            TemplateExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 8)])
        ]);

        List<TemplateExercise> updated =
        [
            TemplateExercise.Create(
                EntityId.New<ExerciseId>(),
                1,
                [PlannedSet.CreateStrengthLike(ExerciseType.Plyometric, 0, 5)])
        ];

        template.Update("Push Day v2", updated);

        Assert.Equal("Push Day v2", template.Name.Value);
        Assert.Single(template.TemplateExercises);
        Assert.Equal(2, template.DomainEvents.Count);
        Assert.IsType<WorkoutTemplateUpdated>(template.DomainEvents.Last());
    }

    [Fact]
    public void Create_WithEmptyExercises_ShouldThrow()
    {
        UserId userId = EntityId.New<UserId>();

        Assert.Throws<ArgumentException>(
            () => WorkoutTemplate.Create(userId, "Empty", []));
    }
}
