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
    private readonly ICurrentUserService _currentUserService;
    private readonly BloomDbContext _context;
    private readonly ILogger<DeleteTemplateCommandHandler> _logger;

    public DeleteTemplateCommandHandler(
        IWorkoutTemplateRepository templateRepository, 
        ILogger<DeleteTemplateCommandHandler> logger, ICurrentUserService currentUserService, BloomDbContext context)
    {
        _templateRepository = templateRepository;
        _logger = logger;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteTemplateComand request, 
        CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || await _context.Users.FindAsync(userId.Value) is null)
            return Result.Failure("User not authenticated or not found");
        
        var template = await _context.WorkoutTemplates.FindAsync(request.Id, ct);
        if (template is null)
            return Result.Failure("Template not found");

        if (template.UserId != userId.Value)
            return Result.Failure("This template does not belong to you");
        
        _context.WorkoutTemplates.Remove(template);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Workout template deleted: {template}", template.Id);
        
        return Result.Success();
    }
}