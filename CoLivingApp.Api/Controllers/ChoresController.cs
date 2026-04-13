using CoLivingApp.Application.Features.Chores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // ОБЯЗАТЕЛЬНО добавить для ClaimTypes

namespace CoLivingApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChoresController : ControllerBase
{
    private readonly IMediator _mediator;
    public ChoresController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChoreCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { choreId = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteChoreCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var secureCommand = command with { ChoreId = id, UserId = userId! }; // Передаем UserId
        var result = await _mediator.Send(secureCommand);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmChoreCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var secureCommand = command with { ChoreId = id, UserId = userId! }; // Передаем UserId
        var result = await _mediator.Send(secureCommand);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectChoreCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var secureCommand = command with { ChoreId = id, UserId = userId! }; // Передаем UserId
        var result = await _mediator.Send(secureCommand);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpGet("{apartmentId}")]
    public async Task<IActionResult> GetList(Guid apartmentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new GetChoresQuery(apartmentId, userId!)); // Передаем UserId
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}