// using Bloom.Application.Common;
// using Bloom.Application.Common.Behaviours;
// using Bloom.Application.Common.Mappings;
// using Bloom.Application.Contracts.Data.Templates;
// using Bloom.Domain.Entity;
// using Bloom.Infrastructure.Persistence;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
//
// namespace Bloom.Application.Queries;
//
// public record GetAllUserTemplatesQuery 
//     : IRequest<Result<List<WorkoutTemplateData>>>
// {
// }
//
// public class GetAllUserTemplatesQueryHandler
//     : IRequestHandler<GetAllUserTemplatesQuery, Result<List<WorkoutTemplateData>>>
// {
//     private readonly BloomDbContext _context;
//     private readonly ICurrentUserService _currentUserService;
//     private readonly ILogger<GetAllUserTemplatesQueryHandler> _logger;
//
//     public GetAllUserTemplatesQueryHandler(
//         BloomDbContext context,
//         ICurrentUserService currentUserService,
//         ILogger<GetAllUserTemplatesQueryHandler> logger)
//     {
//         _context = context;
//         _currentUserService = currentUserService;
//         _logger = logger;
//     }
//
//     public async Task<Result<List<WorkoutTemplateData>>> Handle(
//         GetAllUserTemplatesQuery request,
//         CancellationToken cancellationToken)
//     {
//         var userId = _currentUserService.UserId;
//
//         if (!userId.HasValue || await _context.Users.FindAsync(userId.Value) is null)
//             return Result<List<WorkoutTemplateData>>.Failure("User not authenticated or not found");
//
//         var templates = await _context.WorkoutTemplates
//             .Where(t => t.UserId == userId.Value)
//             .Include(t => t.Exercises)
//             .ThenInclude(e => e.Sets)
//             .ToListAsync(cancellationToken);
//
//         var exerciseIds = templates
//             .SelectMany(t => t.Exercises)
//             .Select(e => e.ExerciseId)
//             .Distinct()
//             .ToList();
//         
//         var exercises = await _context.Exercises
//             .Where(e => exerciseIds.Contains(e.Id))
//             .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);
//
//         
//         var result = templates
//             .Select(t => t.ToDto(exercises))
//             .ToList();
//         
//         _logger.LogInformation(
//             "User found with {TemplateCount} templates.",
//             result.Count);
//
//         return Result<List<WorkoutTemplateData>>.Success(result);
//     }
// }
