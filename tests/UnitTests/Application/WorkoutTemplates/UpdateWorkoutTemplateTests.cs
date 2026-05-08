using Bloom.Application.WorkoutTemplates;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.WorkoutTemplates;

public sealed class UpdateWorkoutTemplateTests : ApplicationTestBase
{
    private async Task<WorkoutTemplate> SeedTemplate(UserId userId)
    {
        var template = WorkoutTemplate.Create(userId, "Push Day",
        [
            TemplateExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 8)])
        ]);

        await WorkoutTemplateRepository.Save(template);
        return template;
    }

    [Fact]
    public async Task Execute_WithOwnedTemplate_ShouldUpdate()
    {
        UserId userId = EntityId.New<UserId>();
        var template = await SeedTemplate(userId);
        var useCase = new UpdateWorkoutTemplate(UnitOfWork, StubCurrentUser.With(userId), CreateLogger<UpdateWorkoutTemplate>());

        var input = new UpdateWorkoutTemplateInput(
            template.Id.Value,
            "Push Day v2",
            [
                new TemplateExerciseInput(
                    Guid.NewGuid(),
                    0,
                    [new PlannedSetInput("Cardio", 0, null, TimeSpan.FromMinutes(20), 5m, "Km")])
            ]);

        var output = await useCase.Execute(input);

        Assert.Equal(template.Id.Value, output.WorkoutTemplateId);
        var saved = await WorkoutTemplateRepository.ById(template.Id);
        Assert.True(saved.HasValue);
        Assert.Equal("Push Day v2", saved.Value.Name.Value);
    }

    [Fact]
    public async Task Execute_WithMissingTemplate_ShouldThrow()
    {
        var useCase = new UpdateWorkoutTemplate(UnitOfWork, StubCurrentUser.Random(), CreateLogger<UpdateWorkoutTemplate>());
        var input = new UpdateWorkoutTemplateInput(
            Guid.NewGuid(),
            "Name",
            [
                new TemplateExerciseInput(
                    Guid.NewGuid(), 0,
                    [new PlannedSetInput("Strength", 0, 5, null, null, null)])
            ]);

        await Assert.ThrowsAsync<WorkoutTemplateNotFoundException>(() => useCase.Execute(input));
    }

    [Fact]
    public async Task Execute_WithUnauthorizedUser_ShouldThrow()
    {
        UserId ownerId = EntityId.New<UserId>();
        var template = await SeedTemplate(ownerId);
        var useCase = new UpdateWorkoutTemplate(UnitOfWork, StubCurrentUser.Random(), CreateLogger<UpdateWorkoutTemplate>());

        var input = new UpdateWorkoutTemplateInput(
            template.Id.Value,
            "Name",
            [
                new TemplateExerciseInput(
                    Guid.NewGuid(), 0,
                    [new PlannedSetInput("Strength", 0, 5, null, null, null)])
            ]);

        await Assert.ThrowsAsync<WorkoutTemplateAccessDeniedException>(() => useCase.Execute(input));
    }
}
