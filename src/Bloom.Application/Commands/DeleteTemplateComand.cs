using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Commands;

public record DeleteTemplateComand(
    Guid Id
) : IRequest<Result>;

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateComand, Result>
{
    private readonly IWorkoutTemplateRepository _templateRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTemplateCommandHandler> _logger;

    public DeleteTemplateCommandHandler(
        IWorkoutTemplateRepository templateRepository, 
        ILogger<DeleteTemplateCommandHandler> logger, 
        ICurrentUserService currentUserService, 
        IUserRepository userRepository)
    {
        _templateRepository = templateRepository;
        _logger = logger;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(
        DeleteTemplateComand request, 
        CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || await _userRepository.GetUserById(userId.Value, ct) is null)
            return Result.Failure("User not authenticated or not found");

        var template = await _templateRepository.GetWorkoutTemplateById(request.Id);
        if (template is null)
            return Result.Failure("Template not found");

        if (template.UserId != userId.Value)
            return Result.Failure("This template does not belong to you");

        await _templateRepository.DeleteWorkoutTemplate(template);
        
        _logger.LogInformation("Workout template deleted: {template}", template.Id);
        
        return Result.Success();
    }
}