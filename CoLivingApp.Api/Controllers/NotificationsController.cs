using System.Security.Claims;
using CoLivingApp.Application.Features.Notifications.Commands;
using CoLivingApp.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] bool unreadOnly = false, [FromQuery] int take = 50)
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new GetMyNotificationsQuery(userId, unreadOnly, take));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new MarkNotificationReadCommand(userId, id));
        return result.IsSuccess ? Ok(new { notificationId = result.Value }) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("admin/personal")]
    public async Task<IActionResult> SendPersonal([FromBody] SendPersonalNotificationRequest request)
    {
        var adminId = CurrentUserId();
        if (adminId == null) return Unauthorized();

        var result = await _mediator.Send(new SendPersonalNotificationCommand(
            adminId,
            request.BuildingId,
            request.TargetUserId,
            request.Title,
            request.Body,
            request.IsImportant));

        return result.IsSuccess
            ? Ok(new { notificationId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("admin/building")]
    public async Task<IActionResult> SendBuilding([FromBody] SendBuildingNotificationRequest request)
    {
        var adminId = CurrentUserId();
        if (adminId == null) return Unauthorized();

        var result = await _mediator.Send(new SendBuildingNotificationCommand(
            adminId,
            request.BuildingId,
            request.Title,
            request.Body,
            request.IsImportant));

        return result.IsSuccess
            ? Ok(new { recipientsCount = result.Value })
            : BadRequest(new { error = result.Error });
    }

    private string? CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record SendPersonalNotificationRequest(
        Guid BuildingId,
        string TargetUserId,
        string Title,
        string Body,
        bool IsImportant);

    public record SendBuildingNotificationRequest(
        Guid BuildingId,
        string Title,
        string Body,
        bool IsImportant);
}
