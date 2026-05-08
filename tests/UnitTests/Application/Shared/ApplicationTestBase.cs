using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.Shared;

public abstract class ApplicationTestBase
{
    protected readonly TestDomainEventPublisher DomainEventPublisher;
    protected readonly InMemoryUserRepository UserRepository;
    protected readonly InMemoryExerciseRepository ExerciseRepository;
    protected readonly InMemoryLoggedWorkoutRepository LoggedWorkoutRepository;
    protected readonly InMemoryWorkoutTemplateRepository WorkoutTemplateRepository;
    protected readonly InMemoryUnitOfWork UnitOfWork;

    protected ApplicationTestBase()
    {
        DomainEventPublisher = new TestDomainEventPublisher();
        UserRepository = new InMemoryUserRepository();
        ExerciseRepository = new InMemoryExerciseRepository();
        LoggedWorkoutRepository = new InMemoryLoggedWorkoutRepository();
        WorkoutTemplateRepository = new InMemoryWorkoutTemplateRepository();

        UnitOfWork = new InMemoryUnitOfWork(DomainEventPublisher);
        UnitOfWork.RegisterRepository<IUserRepository>(UserRepository);
        UnitOfWork.RegisterRepository<IExerciseRepository>(ExerciseRepository);
        UnitOfWork.RegisterRepository<ILoggedWorkoutRepository>(LoggedWorkoutRepository);
        UnitOfWork.RegisterRepository<IWorkoutTemplateRepository>(WorkoutTemplateRepository);
    }

    protected static TestLogger<T> CreateLogger<T>() => new();
}
