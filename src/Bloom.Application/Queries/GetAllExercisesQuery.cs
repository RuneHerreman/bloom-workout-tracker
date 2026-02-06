using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Application.Common.Mappings;
using Bloom.Application.DTO;
using Bloom.Domain.Entity;
using Bloom.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Queries;

public record GetAllExercisesQuery : IRequest<Result<List<ExerciseDTO>>>
{
}

public class GetAllExercisesQueryHandler : IRequestHandler<GetAllExercisesQuery, Result<List<ExerciseDTO>>>
{
    private readonly BloomDbContext _context;
    private readonly ILogger<GetAllExercisesQueryHandler> _logger;

    public GetAllExercisesQueryHandler(BloomDbContext context, ILogger<GetAllExercisesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<ExerciseDTO>>> Handle(GetAllExercisesQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _context.Exercises.ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {exercises} exercises.", exercises.Count);

        return Result<List<ExerciseDTO>>.Success(exercises.Select(e => e.ToDto()).ToList());
    }
}