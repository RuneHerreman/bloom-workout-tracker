using Bloom.Application.Commands;
using Bloom.Application.Common;
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

    [HttpPost("new")]
    public async Task<ActionResult<Result<Guid>>> CreateWorkoutTemplate(
        [FromBody] CreateWorkoutTemplateCommand command
    )
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest( new { error = result.Errors} );

        return Ok(new { id = result.Value });
        
    }
}