using Bloom.Application.Exercises;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Exercises;

public sealed class CustomExerciseUseCasesTests : ApplicationTestBase
{
    private async Task<User> SeedUser()
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    private CreateCustomExercise CreateUseCase(UserId userId) =>
        new(UnitOfWork, StubCurrentUser.With(userId), CreateLogger<CreateCustomExercise>());

    private UpdateCustomExercise UpdateUseCase(UserId userId) =>
        new(UnitOfWork, StubCurrentUser.With(userId), CreateLogger<UpdateCustomExercise>());

    private DeleteCustomExercise DeleteUseCase(UserId userId) =>
        new(UnitOfWork, StubCurrentUser.With(userId), CreateLogger<DeleteCustomExercise>());

    // ------------------------- Create -------------------------

    [Fact]
    public async Task Create_WithValidInput_ShouldSaveOwnedExercise()
    {
        var user = await SeedUser();

        var output = await CreateUseCase(user.Id).Execute(
            new CreateCustomExerciseInput("Weighted Pistol Squat", "A unilateral squat with added load.", "Strength", ["Quadriceps"]));

        var saved = await ExerciseRepository.ById(new ExerciseId(output.ExerciseId));
        Assert.True(saved.HasValue);
        Assert.Equal(user.Id, saved.Value.OwnerUserId);
        Assert.True(saved.Value.IsCustom);
        Assert.Equal(ExerciseType.Strength, saved.Value.Type);
        Assert.Equal("Weighted Pistol Squat", saved.Value.Name.Value);
    }

    [Fact]
    public async Task Create_WithMissingUser_ShouldThrow()
    {
        var useCase = new CreateCustomExercise(UnitOfWork, StubCurrentUser.Random(), CreateLogger<CreateCustomExercise>());

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => useCase.Execute(new CreateCustomExerciseInput("X", "Y", "Strength", ["Chest"])));
    }

    [Fact]
    public async Task Create_WithNameMatchingGlobalCatalog_ShouldThrow()
    {
        var user = await SeedUser();
        await ExerciseRepository.Save(Exercise.Create("Bench Press", "Catalog exercise.", ExerciseType.Strength, ["Chest"]));

        await Assert.ThrowsAsync<ExerciseAlreadyExistsException>(
            () => CreateUseCase(user.Id).Execute(new CreateCustomExerciseInput("Bench Press", "Dup.", "Strength", ["Chest"])));
    }

    [Fact]
    public async Task Create_WithNameMatchingOwnCustom_ShouldThrow()
    {
        var user = await SeedUser();
        await CreateUseCase(user.Id).Execute(new CreateCustomExerciseInput("Super Curl", "First.", "Strength", ["Biceps"]));

        await Assert.ThrowsAsync<ExerciseAlreadyExistsException>(
            () => CreateUseCase(user.Id).Execute(new CreateCustomExerciseInput("Super Curl", "Second.", "Strength", ["Biceps"])));
    }

    [Fact]
    public async Task Create_WithNameMatchingAnotherUsersCustom_ShouldSucceed()
    {
        var userA = await SeedUser();
        var userB = User.Create("b@example.com", "bob", "hash", "Bob", "Jones", 80m, 185, 3, new DateOnly(1992, 5, 5));
        await UserRepository.Save(userB);

        // Both users name a custom exercise the same thing — uniqueness is scoped per creator.
        await CreateUseCase(userA.Id).Execute(new CreateCustomExerciseInput("Super Curl", "A's.", "Strength", ["Biceps"]));
        var output = await CreateUseCase(userB.Id).Execute(new CreateCustomExerciseInput("Super Curl", "B's.", "Strength", ["Biceps"]));

        var saved = await ExerciseRepository.ById(new ExerciseId(output.ExerciseId));
        Assert.True(saved.HasValue);
        Assert.Equal(userB.Id, saved.Value.OwnerUserId);
    }

    // ------------------------- Update -------------------------

    [Fact]
    public async Task Update_OwnedExercise_ShouldApplyChanges()
    {
        var user = await SeedUser();
        var exercise = Exercise.CreateCustom(user.Id, "Old Name", "Old description.", ExerciseType.Strength, ["Chest"]);
        await ExerciseRepository.Save(exercise);

        await UpdateUseCase(user.Id).Execute(
            new UpdateCustomExerciseInput(exercise.Id.Value, "New Name", "New description.", "Cardio", ["Legs", "Core"]));

        var saved = await ExerciseRepository.ById(exercise.Id);
        Assert.Equal("New Name", saved.Value.Name.Value);
        Assert.Equal("New description.", saved.Value.Description.Value);
        Assert.Equal(ExerciseType.Cardio, saved.Value.Type);
        Assert.Equal(2, saved.Value.TargetMuscles.Count);
        Assert.Equal(user.Id, saved.Value.OwnerUserId);
    }

    [Fact]
    public async Task Update_GlobalCatalogExercise_ShouldThrowAccessDenied()
    {
        var user = await SeedUser();
        var exercise = Exercise.Create("Bench Press", "Catalog exercise.", ExerciseType.Strength, ["Chest"]);
        await ExerciseRepository.Save(exercise);

        await Assert.ThrowsAsync<ExerciseAccessDeniedException>(
            () => UpdateUseCase(user.Id).Execute(new UpdateCustomExerciseInput(exercise.Id.Value, "Hacked", "Nope.", "Strength", ["Chest"])));
    }

    [Fact]
    public async Task Update_AnotherUsersExercise_ShouldThrowAccessDenied()
    {
        var user = await SeedUser();
        var foreign = Exercise.CreateCustom(EntityId.New<UserId>(), "Theirs", "Foreign.", ExerciseType.Strength, ["Chest"]);
        await ExerciseRepository.Save(foreign);

        await Assert.ThrowsAsync<ExerciseAccessDeniedException>(
            () => UpdateUseCase(user.Id).Execute(new UpdateCustomExerciseInput(foreign.Id.Value, "Mine Now", "Nope.", "Strength", ["Chest"])));
    }

    [Fact]
    public async Task Update_MissingExercise_ShouldThrowNotFound()
    {
        var user = await SeedUser();

        await Assert.ThrowsAsync<ExerciseNotFoundException>(
            () => UpdateUseCase(user.Id).Execute(new UpdateCustomExerciseInput(Guid.NewGuid(), "X", "Y", "Strength", ["Chest"])));
    }

    // ------------------------- Delete -------------------------

    [Fact]
    public async Task Delete_OwnedExercise_ShouldRemove()
    {
        var user = await SeedUser();
        var exercise = Exercise.CreateCustom(user.Id, "My Custom", "Mine.", ExerciseType.Strength, ["Chest"]);
        await ExerciseRepository.Save(exercise);

        await DeleteUseCase(user.Id).Execute(new DeleteCustomExerciseInput(exercise.Id.Value));

        Assert.False(await ExerciseRepository.Exists(exercise.Id));
    }

    [Fact]
    public async Task Delete_GlobalCatalogExercise_ShouldThrowAccessDenied()
    {
        var user = await SeedUser();
        var exercise = Exercise.Create("Bench Press", "Catalog exercise.", ExerciseType.Strength, ["Chest"]);
        await ExerciseRepository.Save(exercise);

        await Assert.ThrowsAsync<ExerciseAccessDeniedException>(
            () => DeleteUseCase(user.Id).Execute(new DeleteCustomExerciseInput(exercise.Id.Value)));
    }

    [Fact]
    public async Task Delete_MissingExercise_ShouldThrowNotFound()
    {
        var user = await SeedUser();

        await Assert.ThrowsAsync<ExerciseNotFoundException>(
            () => DeleteUseCase(user.Id).Execute(new DeleteCustomExerciseInput(Guid.NewGuid())));
    }
}
