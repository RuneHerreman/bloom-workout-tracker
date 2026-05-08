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

public sealed class DeleteWorkoutTemplateTests : ApplicationTestBase
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
    public async Task Execute_WithOwnedTemplate_ShouldRemove()
    {
        UserId userId = EntityId.New<UserId>();
        var template = await SeedTemplate(userId);
        var useCase = new DeleteWorkoutTemplate(UnitOfWork, StubCurrentUser.With(userId), CreateLogger<DeleteWorkoutTemplate>());

        await useCase.Execute(new DeleteWorkoutTemplateInput(template.Id.Value));

        Assert.False(await WorkoutTemplateRepository.Exists(template.Id));
    }

    [Fact]
    public async Task Execute_WithMissingTemplate_ShouldThrow()
    {
        var useCase = new DeleteWorkoutTemplate(UnitOfWork, StubCurrentUser.Random(), CreateLogger<DeleteWorkoutTemplate>());

        await Assert.ThrowsAsync<WorkoutTemplateNotFoundException>(
            () => useCase.Execute(new DeleteWorkoutTemplateInput(Guid.NewGuid())));
    }

    [Fact]
    public async Task Execute_WithUnauthorizedUser_ShouldThrow()
    {
        UserId ownerId = EntityId.New<UserId>();
        var template = await SeedTemplate(ownerId);
        var useCase = new DeleteWorkoutTemplate(UnitOfWork, StubCurrentUser.Random(), CreateLogger<DeleteWorkoutTemplate>());

        await Assert.ThrowsAsync<WorkoutTemplateAccessDeniedException>(
            () => useCase.Execute(new DeleteWorkoutTemplateInput(template.Id.Value)));
    }
}
