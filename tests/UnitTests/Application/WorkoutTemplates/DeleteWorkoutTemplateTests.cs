using Bloom.Application.WorkoutTemplates;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Shared;

namespace UnitTests.Application.WorkoutTemplates;

public sealed class DeleteWorkoutTemplateTests : ApplicationTestBase
{
    private readonly DeleteWorkoutTemplate _useCase;

    public DeleteWorkoutTemplateTests()
    {
        _useCase = new DeleteWorkoutTemplate(UnitOfWork, CreateLogger<DeleteWorkoutTemplate>());
    }

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

        await _useCase.Execute(new DeleteWorkoutTemplateInput(template.Id.Value, userId.Value));

        Assert.False(await WorkoutTemplateRepository.Exists(template.Id));
    }

    [Fact]
    public async Task Execute_WithMissingTemplate_ShouldThrow()
    {
        await Assert.ThrowsAsync<WorkoutTemplateNotFoundException>(
            () => _useCase.Execute(new DeleteWorkoutTemplateInput(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task Execute_WithUnauthorizedUser_ShouldThrow()
    {
        UserId ownerId = EntityId.New<UserId>();
        var template = await SeedTemplate(ownerId);

        await Assert.ThrowsAsync<WorkoutTemplateAccessDeniedException>(
            () => _useCase.Execute(new DeleteWorkoutTemplateInput(template.Id.Value, Guid.NewGuid())));
    }
}
