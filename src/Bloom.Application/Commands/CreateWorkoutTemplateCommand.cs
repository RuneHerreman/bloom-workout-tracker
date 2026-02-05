using System.Security.Claims;
using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;
using MediatR;

namespace Bloom.Application.Commands;

public record TemplateExerciseSetDto(
    int SetOrder,
    int Reps,
    int? RIR = null
);

public record TemplateExerciseDto(
    Guid ExerciseId,
    int Order,
    List<TemplateExerciseSetDto> Sets
);

public record CreateWorkoutTemplateCommand(
    string Name,
    List<TemplateExerciseDto> Exercises
) : IRequest<Result<Guid>>;

public class CreateWorkoutTemplateCommandHandler : IRequestHandler<CreateWorkoutTemplateCommand, Result<Guid>>
{
    private readonly IWorkoutTemplateRepository _workoutTemplateRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateWorkoutTemplateCommandHandler(
        IWorkoutTemplateRepository workoutTemplateRepository,
        IExerciseRepository exerciseRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _workoutTemplateRepository = workoutTemplateRepository;
        _exerciseRepository = exerciseRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateWorkoutTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || await _userRepository.GetUserById(userId.Value, cancellationToken) is null)
            return Result<Guid>.Failure("User not authenticated or not found");

        var exerciseIds = request.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var exercises = await _exerciseRepository.GetByIdsAsync(exerciseIds, cancellationToken);
    
        if (exercises.Count != exerciseIds.Count)
            return Result<Guid>.Failure("One or more exercises not found");

        var templateExercises = request.Exercises.Select(dto =>
        {
            var exercise = new WorkoutTemplateExercise
            {
                Id = Guid.NewGuid(),
                ExerciseId = dto.ExerciseId,
                Order = dto.Order
            };

            foreach (var s in dto.Sets)
            {
                exercise.Sets.Add(new TemplateExerciseSet
                {
                    SetOrder = s.SetOrder,
                    Reps = s.Reps,
                    RIR = s.RIR,
                    WorkoutTemplateExercise = exercise
                });
            }

            return exercise;
        }).ToList();
        
        var template = new WorkoutTemplate
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            Name = request.Name,
            Exercises = templateExercises
        };
        
        await _workoutTemplateRepository.AddWorkoutTemplate(template);

        return Result<Guid>.Success(template.Id);
    }
}