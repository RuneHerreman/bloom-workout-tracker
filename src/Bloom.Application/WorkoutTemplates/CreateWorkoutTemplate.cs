using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Domain.WorkoutTemplates.ValueObjects;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.WorkoutTemplates;

public sealed record PlannedSetInput(
    string Type,
    int Order,
    int? Reps,
    TimeSpan? Duration,
    decimal? Distance,
    string? DistanceUnit
);

public sealed record TemplateExerciseInput(
    Guid ExerciseId,
    int Order,
    List<PlannedSetInput> Sets
);

public sealed record CreateWorkoutTemplateInput(
    Guid UserId,
    string Name,
    List<TemplateExerciseInput> Exercises
);

public sealed record CreateWorkoutTemplateOutput(Guid WorkoutTemplateId);

public class CreateWorkoutTemplate(
    IUnitOfWork uow,
    ILogger<CreateWorkoutTemplate> logger
) : IUseCase<CreateWorkoutTemplateInput, CreateWorkoutTemplateOutput>
{
    public async Task<CreateWorkoutTemplateOutput> Execute(CreateWorkoutTemplateInput input)
    {
        logger.LogInformation($"Creating WorkoutTemplate | User: {input.UserId}");

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var userExists = await uow.Repo<IUserRepository>().Exists(EntityId.New<UserId>(input.UserId));

        if (userExists)
            throw new UserNotFoundException($"User not found | Id: {input.UserId}");

        var exercises = input.Exercises.Select(MapExercise).ToList();

        var template = WorkoutTemplate.Create(
            EntityId.New<UserId>(input.UserId),
            input.Name,
            exercises
        );

        await templateRepo.Save(template);
        await uow.Do();

        logger.LogInformation($"WorkoutTemplate created | Id: {template.Id} - User: {input.UserId}");
        
        return new CreateWorkoutTemplateOutput(template.Id.Value);
    }

    private static TemplateExercise MapExercise(TemplateExerciseInput e)
    {
        var sets = e.Sets.Select(MapSet).ToList();
        return TemplateExercise.Create(EntityId.New<ExerciseId>(e.ExerciseId), e.Order, sets);
    }

    private static PlannedSet MapSet(PlannedSetInput s)
    {
        var type = Enum.Parse<ExerciseType>(s.Type, ignoreCase: true);

        return type switch
        {
            ExerciseType.Cardio => PlannedSet.CreateCardio(
                s.Order,
                s.Duration ?? throw new ArgumentException("Duration required for Cardio set"),
                s.Distance ?? throw new ArgumentException("Distance required for Cardio set"),
                Enum.Parse<PlannedDistanceUnit>(s.DistanceUnit ?? throw new ArgumentException("DistanceUnit required for Cardio set"), ignoreCase: true)
            ),
            ExerciseType.Strength or ExerciseType.Plyometric => PlannedSet.CreateStrengthLike(
                type,
                s.Order,
                s.Reps ?? throw new ArgumentException("Reps required for Strength/Plyometric set")
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(s.Type), s.Type, "Unsupported exercise type")
        };
    }
}