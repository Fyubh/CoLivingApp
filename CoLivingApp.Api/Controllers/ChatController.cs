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
        var result = await _mediator.Send(new GetChatHistoryQuery(apartmentId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new SendMessageCommand(request.ApartmentId, userId!, request.Text);
        
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            // Отправляем сообщение ТОЛЬКО жильцам этой квартиры
            await _hub.Clients.Group(request.ApartmentId.ToString()).SendAsync("ReceiveChatMessage", result.Value);
            return Ok();
        }
        return BadRequest(new { error = result.Error });
    }
}

public record SendMessageRequest(Guid ApartmentId, string Text);