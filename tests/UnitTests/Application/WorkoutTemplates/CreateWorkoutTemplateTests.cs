using Bloom.Application.WorkoutTemplates;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.WorkoutTemplates;

public sealed class CreateWorkoutTemplateTests : ApplicationTestBase
{
    private static CreateWorkoutTemplateInput BuildInput()
    {
        return new CreateWorkoutTemplateInput(
            "Push Day",
            [
                new TemplateExerciseInput(
                    Guid.NewGuid(),
                    0,
                    [new PlannedSetInput("Strength", 0, 8, null, null, null)])
            ]);
    }

    [Fact]
    public async Task Execute_WithExistingUser_ShouldCreateTemplate()
    {
        User user = User.Create("user@example.com", "alice", "hash", 72.5m, 180, 4);
        await UserRepository.Save(user);
        var useCase = new CreateWorkoutTemplate(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<CreateWorkoutTemplate>());

        var output = await useCase.Execute(BuildInput());

        Assert.NotEqual(Guid.Empty, output.WorkoutTemplateId);
        var saved = await WorkoutTemplateRepository.ById(EntityId.New<Bloom.Domain.WorkoutTemplates.WorkoutTemplateId>(output.WorkoutTemplateId));
        Assert.True(saved.HasValue);
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        var useCase = new CreateWorkoutTemplate(UnitOfWork, StubCurrentUser.Random(), CreateLogger<CreateWorkoutTemplate>());

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => useCase.Execute(BuildInput()));
    }
}
