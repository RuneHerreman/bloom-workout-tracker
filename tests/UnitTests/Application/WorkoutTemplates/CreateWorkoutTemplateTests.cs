using Bloom.Application.WorkoutTemplates;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Shared;

namespace UnitTests.Application.WorkoutTemplates;

public sealed class CreateWorkoutTemplateTests : ApplicationTestBase
{
    private readonly CreateWorkoutTemplate _useCase;

    public CreateWorkoutTemplateTests()
    {
        _useCase = new CreateWorkoutTemplate(UnitOfWork, CreateLogger<CreateWorkoutTemplate>());
    }

    private static CreateWorkoutTemplateInput BuildInput(Guid userId)
    {
        return new CreateWorkoutTemplateInput(
            userId,
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
        User user = User.Create("user@example.com", "alice", "hash");
        await UserRepository.Save(user);

        var output = await _useCase.Execute(BuildInput(user.Id.Value));

        Assert.NotEqual(Guid.Empty, output.WorkoutTemplateId);
        var saved = await WorkoutTemplateRepository.ById(EntityId.New<Bloom.Domain.WorkoutTemplates.WorkoutTemplateId>(output.WorkoutTemplateId));
        Assert.True(saved.HasValue);
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        await Assert.ThrowsAsync<UserNotFoundException>(
            () => _useCase.Execute(BuildInput(Guid.NewGuid())));
    }
}
