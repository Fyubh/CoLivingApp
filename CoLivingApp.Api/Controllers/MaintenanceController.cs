using System.Security.Claims;
using CoLivingApp.Application.Features.Maintenance.Commands.CreateMaintenanceRequest;
using CoLivingApp.Application.Features.Maintenance.Queries.GetMyMaintenanceRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMediator _mediator;
    public MaintenanceController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Создать заявку на ремонт. ReportedByUserId подставляется из JWT,
    /// клиент передаёт в теле пустую строку (или любое значение — будет перезаписано).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var secureCommand = command with { ReportedByUserId = userId };
        var result = await _mediator.Send(secureCommand);

        return result.IsSuccess
            ? Ok(new { maintenanceRequestId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Список заявок текущего жильца.
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetMyMaintenanceRequestsQuery(userId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}