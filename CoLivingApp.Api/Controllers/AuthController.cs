using CoLivingApp.Application.Features.Users.Commands.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { userId = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { token = result.Value }) : Unauthorized(new { error = result.Error });
    }
}