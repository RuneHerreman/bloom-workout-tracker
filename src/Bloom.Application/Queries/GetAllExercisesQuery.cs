using Bloom.Application.Common;
using Bloom.Domain.Entity;
using Bloom.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Application.Queries;

public record GetAllExercisesQuery : IRequest<Result<List<Exercise>>>
{
}

public class GetAllExercisesQueryHandler : IRequestHandler<GetAllExercisesQuery, Result<List<Exercise>>>
{
    private readonly BloomDbContext _context;

    public GetAllExercisesQueryHandler(BloomDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<Exercise>>> Handle(GetAllExercisesQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _context.Exercises.ToListAsync(cancellationToken);
        return Result<List<Exercise>>.Success(exercises);
    }
}