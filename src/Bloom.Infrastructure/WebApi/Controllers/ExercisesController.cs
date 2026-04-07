using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers;

[ApiController]
[Route("api/exercises")]
public class ExercisesController : ControllerBase
{
    // private readonly IMediator _mediator;
    //
    // public ExercisesController(IMediator mediator) => _mediator = mediator;
    //
    // [HttpGet]
    // [EndpointSummary("Gets a list of exercises.")]
    // public async Task<ActionResult<Result<List<ExerciseDTO>>>> GetExercises()
    // {
    //     var result = await _mediator.Send(new GetAllExercisesQuery());
    //     return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    // }
}
