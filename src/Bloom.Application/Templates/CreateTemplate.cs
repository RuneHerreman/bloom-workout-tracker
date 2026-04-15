using Bloom.Application.Contracts.Data.Templates;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;
using Bloom.Domain.Templates;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Templates;

public record CreateWorkoutTemplateInput(
    Guid UserId,
    string Name,
    List<WorkoutTemplateExerciseData> Exercises
);


public class CreateTemplate(
    IUnitOfWork uow,
    ILogger<CreateTemplate> logger
): IUseCase<CreateWorkoutTemplateInput, WorkoutTemplateId>
{
    public async Task<WorkoutTemplateId> Execute(CreateWorkoutTemplateInput input)
    {
        var userRepository = uow.Repo<IUserRepository>();
        var exerciseRepository = uow.Repo<IExerciseRepository>();

        var exists = await userRepository.Exists(EntityId.New<UserId>(input.UserId));
        
        if (!exists)
            throw new UserDoesNotExistError($"User not found.");

        if (input.Exercises.Count == 0)
            throw new InvalidWorkoutTemplateException("A workout template must contain at least one exercise.");

        var template = WorkoutTemplate.Create(
            EntityId.New<UserId>(input.UserId),
            input.Name
        );

        foreach (var exerciseInput in input.Exercises)
        {
            var exerciseId = EntityId.New<ExerciseId>(exerciseInput.ExerciseId);
            if (!await exerciseRepository.Exists(exerciseId))
                throw new InvalidWorkoutTemplateException($"Exercise {exerciseInput.ExerciseId} was not found.");

            var exercise = await exerciseRepository.ById(exerciseId);
            var templateExercise = WorkoutTemplateExercise.Create(
                template.Id,
                exerciseId,
                exerciseInput.Order
            );

            if (exerciseInput.Sets.Count == 0)
                throw new InvalidWorkoutTemplateException("Each exercise must contain at least one set.");

            foreach (var setInput in exerciseInput.Sets)
            {
                ValidateSetInput(setInput);

                switch (exercise.Type)
                {
                    case ExerciseType.Strength:
                        templateExercise.AddSet(MapStrengthSet(templateExercise.Id, setInput));
                        break;
                    case ExerciseType.Plyometric:
                        templateExercise.AddSet(MapStrengthSet(templateExercise.Id, setInput));
                        break;
                    case ExerciseType.Cardio:
                        templateExercise.AddSet(MapCardioSet(templateExercise.Id, setInput));
                        break;
                    default:
                        throw new InvalidWorkoutTemplateException(
                            $"Exercise type {exercise.Type} is not supported for template creation.");
                }
            }

            template.AddExercise(templateExercise);
        }

        await uow.Save<IWorkoutTemplateRepository>(template);
        await uow.Do();
        logger.LogInformation("Created workout template {TemplateId} with {ExerciseCount} exercise(s)", template.Id, template.Exercises.Count);

        return new WorkoutTemplateId(template.Id.Value);
    }

    private static TemplateStrengthSet MapStrengthSet(WorkoutTemplateExerciseId templateExerciseId, TemplateExerciseSetData input)
    {
        if (!input.Reps.HasValue)
            throw new InvalidWorkoutTemplateException("Strength sets require reps.");

        return TemplateStrengthSet.Create(
            templateExerciseId,
            input.SetOrder,
            input.Reps.Value,
            input.RIR ?? 0
        );
    }

    private static TemplateCardioSet MapCardioSet(WorkoutTemplateExerciseId templateExerciseId, TemplateExerciseSetData input)
    {
        return TemplateCardioSet.Create(
            templateExerciseId,
            input.Duration ?? TimeOnly.MinValue,
            input.Distance ?? 0
        );
    }

    private static void ValidateSetInput(TemplateExerciseSetData setInput)
    {
        var hasStrengthValues = setInput.Reps.HasValue || setInput.RIR.HasValue;
        var hasCardioValues = setInput.Duration.HasValue || setInput.Distance.HasValue;

        if (hasStrengthValues && hasCardioValues)
            throw new InvalidWorkoutTemplateException("A set cannot define both strength and cardio fields.");

        if (!hasStrengthValues && !hasCardioValues)
            throw new InvalidWorkoutTemplateException("A set must define either strength or cardio fields.");
    }
}