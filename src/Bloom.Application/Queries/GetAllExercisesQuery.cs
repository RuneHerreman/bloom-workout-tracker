using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Domain.Entity;
using Bloom.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Queries;

public record GetAllExercisesQuery : IRequest<Result<List<Exercise>>>
{
}

public class GetAllExercisesQueryHandler : IRequestHandler<GetAllExercisesQuery, Result<List<Exercise>>>
{
    private readonly BloomDbContext _context;
    private readonly ILogger<GetAllExercisesQueryHandler> _logger;

    public GetAllExercisesQueryHandler(BloomDbContext context, ILogger<GetAllExercisesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Exercise>>> Handle(GetAllExercisesQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _context.Exercises.ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {exercises} exercises.", exercises.Count);
        return Result<List<Exercise>>.Success(exercises);
    }
}