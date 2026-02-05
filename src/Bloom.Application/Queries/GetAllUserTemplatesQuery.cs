using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Domain.Entity;
using Bloom.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Queries;

public record GetAllUserTemplatesQuery 
    : IRequest<Result<List<WorkoutTemplate>>>
{
}

public class GetAllUserTemplatesQueryHandler
    :IRequestHandler<GetAllUserTemplatesQuery, Result<List<WorkoutTemplate>>>
{
    private readonly BloomDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAllUserTemplatesQueryHandler> _logger;

    
    public GetAllUserTemplatesQueryHandler(
        BloomDbContext context, 
        ICurrentUserService currentUserService, ILogger<GetAllUserTemplatesQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    
    public async Task<Result<List<WorkoutTemplate>>> Handle(GetAllUserTemplatesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || await _context.Users.FindAsync(userId.Value) is null)
            return Result<List<WorkoutTemplate>>.Failure("User not authenticated or not found");
        
        var templates = await _context.WorkoutTemplates.Where(t => t.UserId == userId).ToListAsync(cancellationToken);
        _logger.LogInformation("User found with {templates} templates.", templates.Count);
        return Result<List<WorkoutTemplate>>.Success(templates);
    }
}