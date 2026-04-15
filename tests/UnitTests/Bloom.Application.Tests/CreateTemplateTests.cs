using Bloom.Application.Contracts.Data.Templates;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Templates;
using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;
using Bloom.Domain.Templates;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests.Bloom.Application.Tests;

public class CreateTemplateTests
{
    [Fact]
    public async Task Execute_WithStrengthExercise_MapsStrengthSets()
    {
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create("Bench Press", "Press", ExerciseType.Strength, "Chest", EntityId.New<ExerciseId>(exerciseId));

        var userRepository = new FakeUserRepository(userId);
        var exerciseRepository = new FakeExerciseRepository(exercise);
        var templateRepository = new FakeWorkoutTemplateRepository();
        var uow = new FakeUnitOfWork(userRepository, exerciseRepository, templateRepository);
        var useCase = new CreateTemplate(uow, new MockLogger<CreateTemplate>());

        var input = new CreateWorkoutTemplateInput(
            userId,
            "Upper Body",
            [
                new WorkoutTemplateExerciseData(
                    exerciseId,
                    "Bench Press",
                    0,
                    [new TemplateExerciseSetData(0, Reps: 8, RIR: 2)]
                )
            ]
        );

        var result = await useCase.Execute(input);

        Assert.NotEqual(default, result);
        Assert.NotNull(templateRepository.SavedTemplate);
        Assert.Single(templateRepository.SavedTemplate!.Exercises);
        Assert.Single(templateRepository.SavedTemplate.Exercises[0].StrengthSets);
        Assert.Empty(templateRepository.SavedTemplate.Exercises[0].CardioSets);
        Assert.True(uow.DidCommit);
    }

    [Fact]
    public async Task Execute_WithCardioExercise_MapsCardioSets()
    {
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create("Run", "Run", ExerciseType.Cardio, "Legs", EntityId.New<ExerciseId>(exerciseId));

        var userRepository = new FakeUserRepository(userId);
        var exerciseRepository = new FakeExerciseRepository(exercise);
        var templateRepository = new FakeWorkoutTemplateRepository();
        var uow = new FakeUnitOfWork(userRepository, exerciseRepository, templateRepository);
        var useCase = new CreateTemplate(uow, new MockLogger<CreateTemplate>());

        var input = new CreateWorkoutTemplateInput(
            userId,
            "Cardio Day",
            [
                new WorkoutTemplateExerciseData(
                    exerciseId,
                    "Run",
                    0,
                    [new TemplateExerciseSetData(0, Duration: new TimeOnly(0, 20), Distance: 3.5m)]
                )
            ]
        );

        await useCase.Execute(input);

        Assert.NotNull(templateRepository.SavedTemplate);
        Assert.Single(templateRepository.SavedTemplate!.Exercises);
        Assert.Single(templateRepository.SavedTemplate.Exercises[0].CardioSets);
        Assert.Empty(templateRepository.SavedTemplate.Exercises[0].StrengthSets);
        Assert.True(uow.DidCommit);
    }

    [Fact]
    public async Task Execute_WithMixedSetFields_Throws()
    {
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create("Run", "Run", ExerciseType.Cardio, "Legs", EntityId.New<ExerciseId>(exerciseId));

        var userRepository = new FakeUserRepository(userId);
        var exerciseRepository = new FakeExerciseRepository(exercise);
        var templateRepository = new FakeWorkoutTemplateRepository();
        var uow = new FakeUnitOfWork(userRepository, exerciseRepository, templateRepository);
        var useCase = new CreateTemplate(uow, new MockLogger<CreateTemplate>());

        var input = new CreateWorkoutTemplateInput(
            userId,
            "Invalid",
            [
                new WorkoutTemplateExerciseData(
                    exerciseId,
                    "Run",
                    0,
                    [new TemplateExerciseSetData(0, Reps: 10, Duration: new TimeOnly(0, 30))]
                )
            ]
        );

        await Assert.ThrowsAsync<InvalidWorkoutTemplateException>(() => useCase.Execute(input));
        Assert.False(uow.DidCommit);
    }

    [Fact]
    public void WorkoutTemplateExercise_AddingBothSetTypes_Throws()
    {
        var template = WorkoutTemplate.Create(EntityId.New<UserId>(Guid.NewGuid()), "Template");
        var templateExercise = WorkoutTemplateExercise.Create(template.Id, EntityId.New<ExerciseId>(Guid.NewGuid()), 0);

        templateExercise.AddSet(TemplateStrengthSet.Create(templateExercise.Id, 0, 10, 2));

        Assert.Throws<InvalidOperationException>(
            () => templateExercise.AddSet(TemplateCardioSet.Create(templateExercise.Id, new TimeOnly(0, 10), 1.5m)));
    }
}

internal sealed class FakeUnitOfWork(
    FakeUserRepository userRepository,
    FakeExerciseRepository exerciseRepository,
    FakeWorkoutTemplateRepository templateRepository
) : IUnitOfWork
{
    public bool DidCommit { get; private set; }

    public Task Do()
    {
        DidCommit = true;
        return Task.CompletedTask;
    }

    public Task Save<TRepository>(IAggregateRoot aggregateRoot) where TRepository : IRepository
    {
        var repository = Repo<TRepository>();
        return ((dynamic)repository).Save((dynamic)aggregateRoot);
    }

    public TRepository Repo<TRepository>() where TRepository : IRepository
    {
        if (typeof(TRepository) == typeof(IUserRepository))
            return (TRepository)(IRepository)userRepository;

        if (typeof(TRepository) == typeof(IExerciseRepository))
            return (TRepository)(IRepository)exerciseRepository;

        if (typeof(TRepository) == typeof(IWorkoutTemplateRepository))
            return (TRepository)(IRepository)templateRepository;

        throw new InvalidOperationException($"Repository not registered: {typeof(TRepository).Name}");
    }
}

internal sealed class FakeUserRepository(Guid existingUserId) : IUserRepository
{
    private readonly UserId _existingUserId = EntityId.New<UserId>(existingUserId);

    public Task<bool> Exists(UserId id) => Task.FromResult(id.Equals(_existingUserId));
    public Task<User> ById(UserId id) => throw new NotImplementedException();
    public Task Save(User aggregateRoot) => Task.CompletedTask;
    public Task Remove(User aggregateRoot) => Task.CompletedTask;
    public Task<User?> GetUserByEmail(string email) => Task.FromResult<User?>(null);
}

internal sealed class FakeExerciseRepository(params Exercise[] exercises) : IExerciseRepository
{
    private readonly Dictionary<ExerciseId, Exercise> _exercises = exercises.ToDictionary(e => e.Id, e => e);

    public Task<bool> Exists(ExerciseId id) => Task.FromResult(_exercises.ContainsKey(id));

    public Task<Exercise> ById(ExerciseId id)
    {
        if (!_exercises.TryGetValue(id, out var exercise))
            throw new InvalidOperationException("Exercise not found");

        return Task.FromResult(exercise);
    }

    public Task Save(Exercise aggregateRoot) => Task.CompletedTask;
    public Task Remove(Exercise aggregateRoot) => Task.CompletedTask;
}

internal sealed class FakeWorkoutTemplateRepository : IWorkoutTemplateRepository
{
    public WorkoutTemplate? SavedTemplate { get; private set; }

    public Task<bool> Exists(WorkoutTemplateId id) => Task.FromResult(SavedTemplate?.Id.Equals(id) == true);

    public Task<WorkoutTemplate> ById(WorkoutTemplateId id)
    {
        if (SavedTemplate is null)
            throw new InvalidOperationException("Template not found");

        return Task.FromResult(SavedTemplate);
    }

    public Task Save(WorkoutTemplate aggregateRoot)
    {
        SavedTemplate = aggregateRoot;
        return Task.CompletedTask;
    }

    public Task Remove(WorkoutTemplate aggregateRoot)
    {
        if (SavedTemplate?.Id.Equals(aggregateRoot.Id) == true)
            SavedTemplate = null;

        return Task.CompletedTask;
    }

    public Task AddWorkoutTemplate(WorkoutTemplate template)
    {
        SavedTemplate = template;
        return Task.CompletedTask;
    }

    public Task DeleteWorkoutTemplate(WorkoutTemplate template) => Remove(template);

    public Task<WorkoutTemplate?> GetWorkoutTemplateById(WorkoutTemplateId id, UserId userId)
    {
        if (SavedTemplate is null)
            return Task.FromResult<WorkoutTemplate?>(null);

        return Task.FromResult(SavedTemplate.Id.Equals(id) && SavedTemplate.UserId.Equals(userId) ? SavedTemplate : null);
    }

    public Task UpdateWorkoutTemplate(WorkoutTemplateId templateId, string name, List<WorkoutTemplateExercise> newExercises, UserId userId)
    {
        throw new NotImplementedException();
    }
}

