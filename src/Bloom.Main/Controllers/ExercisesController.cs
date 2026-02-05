using Bloom.Application.Common;
using Bloom.Application.Queries;
using Bloom.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Main.Controllers;

[ApiController]
[Route("api/exercises")]
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExercisesController(IMediator mediator) => _mediator = mediator;
    
    [HttpGet]
    public async Task<ActionResult<Result<List<Exercise>>>> GetExercises()
    {
        var result = await _mediator.Send(new GetAllExercisesQuery());
        return Ok(result);
    }
}
