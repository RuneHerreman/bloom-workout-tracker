using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Application.Common.Mappings;
using Bloom.Application.DTO.Templates;
using Bloom.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Queries;

public record GetTemplateByIdQuery (
    Guid TemplateId
): IRequest<Result<WorkoutTemplateDTO>>;

public class GetTemplateByIdQueryHandler
    : IRequestHandler<GetTemplateByIdQuery, Result<WorkoutTemplateDTO>>
{
    private readonly BloomDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTemplateByIdQueryHandler> _logger;

    public GetTemplateByIdQueryHandler(
        BloomDbContext context, 
        ICurrentUserService currentUserService, 
        ILogger<GetTemplateByIdQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }


    public async Task<Result<WorkoutTemplateDTO>> Handle(
        GetTemplateByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue || await _context.Users.FindAsync(userId.Value) is null)
            return Result<WorkoutTemplateDTO>.Failure("User not authenticated or not found");
        
        var template = await _context.WorkoutTemplates
            .Include(t => t.Exercises)
            .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(t => t.UserId == userId.Value && t.Id == request.TemplateId, cancellationToken);

        if (template == null)
            return Result<WorkoutTemplateDTO>.Failure("Template not found");
        
        var exerciseIds = template.Exercises
            .Select(e => e.ExerciseId)
            .Distinct()
            .ToList();

        var exerciseNames = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);
        
        _logger.LogInformation("Template found: {template}", template.Id);
        
        return Result<WorkoutTemplateDTO>.Success(template.ToDto(exerciseNames));
    }
}