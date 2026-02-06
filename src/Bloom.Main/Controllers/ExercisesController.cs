using Bloom.Application.Common;
using Bloom.Application.DTO;
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
    [EndpointSummary("Gets a list of exercises.")]
    public async Task<ActionResult<Result<List<ExerciseDTO>>>> GetExercises()
    {
        var result = await _mediator.Send(new GetAllExercisesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}
