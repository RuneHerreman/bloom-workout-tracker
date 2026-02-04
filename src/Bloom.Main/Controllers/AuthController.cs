using Bloom.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Main.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register(RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest( new { error = result.Errors} );

        return Ok(new { token = result.Value });
    }
    
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return Ok(new { token = result.Value });
        
        return Unauthorized(new { error = result.Errors.FirstOrDefault() });
    }
}