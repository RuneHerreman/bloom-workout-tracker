using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Commands;

public record UpdateWorkoutTemplateCommand(
    Guid Id,
    CreateWorkoutTemplateCommand Template
) : IRequest<Result>;

public class UpdateWorkoutTemplateCommandHandler : IRequestHandler<UpdateWorkoutTemplateCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateWorkoutTemplateCommandHandler> _logger;
    private readonly IWorkoutTemplateRepository _templateRepository;
    private readonly IUserRepository _userRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public UpdateWorkoutTemplateCommandHandler(
        ICurrentUserService currentUserService, 
        ILogger<UpdateWorkoutTemplateCommandHandler> logger, 
        IWorkoutTemplateRepository templateRepository, 
        IExerciseRepository exerciseRepository, 
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _templateRepository = templateRepository;
        _exerciseRepository = exerciseRepository;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(UpdateWorkoutTemplateCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || await _userRepository.GetUserById(userId.Value, ct) is null)
            return Result.Failure("User not authenticated or not found");
        
        var template = await _templateRepository.GetWorkoutTemplateById(request.Id);
        if (template is null)
            return Result.Failure("Template not found");
        if (template.UserId != userId.Value)
            return Result.Failure("This template does not belong to you");

        await _templateRepository.DeleteWorkoutTemplateExercises(template.Exercises);
        
        var exerciseIds = request.Template.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var exercises = await _exerciseRepository.GetByIdsAsync(exerciseIds, ct);
        if (exercises.Count != exerciseIds.Count)
            return Result.Failure("One or more exercises not found");
        
        var newTemplateExercises = request.Template.Exercises.Select(dto =>
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
        
        template.Name = request.Template.Name;
        template.Exercises.Clear();
        template.Exercises = newTemplateExercises;
        
        await _templateRepository.UpdateWorkoutTemplate(template);
        _logger.LogInformation("Workout template {TemplateId} updated by user {UserId}", request.Id, userId.Value);
        return Result.Success();
    }
}