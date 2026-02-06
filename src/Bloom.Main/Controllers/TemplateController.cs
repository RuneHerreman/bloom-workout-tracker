using Bloom.Application.Commands;
using Bloom.Application.Common;
using Bloom.Application.DTO.Templates;
using Bloom.Application.Queries;
using Bloom.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Main.Controllers;


[ApiController]
[Route("api/templates")]
[Authorize]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [EndpointDescription("Creates a new workout template.")]
    public async Task<ActionResult<Result<Guid>>> CreateWorkoutTemplate(
        [FromBody] CreateWorkoutTemplateCommand command
    )
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest( new { error = result.Errors} );

        return result.IsSuccess 
            ? CreatedAtAction(
                nameof(GetWorkoutTemplateById),
                new { id = result.Value },
                result.Value
            ) 
            : BadRequest(result.Errors);
        
    }

    [HttpGet]
    [EndpointDescription("Gets all workout templates for the current user.")]
    public async Task<ActionResult<Result<List<WorkoutTemplateDTO>>>> GetWorkoutTemplates()
    {
        var result = await _mediator.Send(new GetAllUserTemplatesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
    
    [HttpGet("{id}")]
    [EndpointDescription("Gets a workout template by ID.")]
    public async Task<ActionResult<Result<List<WorkoutTemplateDTO>>>> GetWorkoutTemplateById(
        [FromRoute] Guid id
    )
    {
        var result = await _mediator.Send(new GetTemplateByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
    }
}