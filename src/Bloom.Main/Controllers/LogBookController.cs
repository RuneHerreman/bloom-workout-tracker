using Bloom.Application.Common;
using Bloom.Application.DTO.LogBook;
using Bloom.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Main.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class LogBookController : ControllerBase
{
    private readonly IMediator _mediator;
    public LogBookController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [EndpointSummary("Gets all logs for the current user")]
    public async Task<ActionResult<Result<List<LoggedWorkoutDTO>>>> GetAllUserLogs()
    {
        var result = await _mediator.Send(new GetAllUserLogsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
    }
}