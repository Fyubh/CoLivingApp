using CoLivingApp.Application.Features.Chat.Commands;
using CoLivingApp.Application.Features.Chat.Queries;
using CoLivingApp.Api.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CoLivingApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<CoLivingHub> _hub;

    public ChatController(IMediator mediator, IHubContext<CoLivingHub> hub)
    {
        _mediator = mediator;
        _hub = hub;
    }

    [HttpGet("{apartmentId}")]
    public async Task<IActionResult> GetHistory(Guid apartmentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new GetChatHistoryQuery(apartmentId, userId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var command = new SendMessageCommand(request.ApartmentId, userId, request.Text);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            // Отправляем сообщение ТОЛЬКО жильцам этой квартиры
            await _hub.Clients.Group(request.ApartmentId.ToString()).SendAsync("ReceiveChatMessage", result.Value);
            return Ok();
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpDelete("messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new DeleteMessageCommand(messageId, userId));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("messages/{messageId}/report")]
    public async Task<IActionResult> ReportMessage(Guid messageId, [FromBody] ReportMessageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new ReportMessageCommand(messageId, userId, request?.Reason));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("blocks")]
    public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new BlockUserCommand(userId, request.BlockedUserId));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("blocks/{blockedUserId}")]
    public async Task<IActionResult> UnblockUser(string blockedUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new UnblockUserCommand(userId, blockedUserId));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}

public record SendMessageRequest(Guid ApartmentId, string Text);
public record ReportMessageRequest(string? Reason);
public record BlockUserRequest(string BlockedUserId);
