using System.Security.Claims;
using CoLivingApp.Api.Hubs;
using CoLivingApp.Application.Features.Maintenance.Commands.AcknowledgeMaintenance;
using CoLivingApp.Application.Features.Maintenance.Commands.AssignMaintenance;
using CoLivingApp.Application.Features.Maintenance.Commands.CancelMaintenance;
using CoLivingApp.Application.Features.Maintenance.Commands.CompleteMaintenance;
using CoLivingApp.Application.Features.Maintenance.Commands.CreateMaintenanceRequest;
using CoLivingApp.Application.Features.Maintenance.Commands.RateMaintenance;
using CoLivingApp.Application.Features.Maintenance.Commands.StartMaintenance;
using CoLivingApp.Application.Features.Maintenance.Queries.GetAvailableWork;
using CoLivingApp.Application.Features.Maintenance.Queries.GetBuildingMaintenanceRequests;
using CoLivingApp.Application.Features.Maintenance.Queries.GetMyAssignedWork;
using CoLivingApp.Application.Features.Maintenance.Queries.GetMyMaintenanceRequests;
using CoLivingApp.Application.Features.Maintenance.Shared;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CoLivingApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<CoLivingHub> _hub;

    public MaintenanceController(IMediator mediator, IHubContext<CoLivingHub> hub)
    {
        _mediator = mediator;
        _hub = hub;
    }

    // ==================================================================================
    // TENANT (жилец)
    // ==================================================================================

    /// <summary>Создать заявку на ремонт.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestCommand command)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(command with { ReportedByUserId = userId });
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });

        // Пуш админам здания: появилась новая заявка.
        // Сначала вытащим BuildingId из созданной заявки — для этого сделаем второй тонкий запрос.
        // Для экономии запросов лучше всего было бы, чтобы CreateMaintenanceRequest тоже возвращал
        // BuildingId, но команда вернётся в текущую сессию — пока обойдёмся одним дополнительным запросом.
        // Проще всего через dispatch по событию из handler'а, но шину событий не вводим ради MVP.
        // TODO (phase 4): сделать CreateMaintenanceRequest возвращающим MaintenanceActionResult.

        return Ok(new { maintenanceRequestId = result.Value });
    }

    /// <summary>Список моих заявок (для жильца).</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new GetMyMaintenanceRequestsQuery(userId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Отменить свою заявку (пока подрядчик не начал работу).</summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBody body)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new CancelMaintenanceCommand(userId, id, body?.Reason));
        return await DispatchOrBadRequest(result);
    }

    /// <summary>Оценить закрытую заявку (1-5).</summary>
    [HttpPost("{id}/rate")]
    public async Task<IActionResult> Rate(Guid id, [FromBody] RateBody body)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new RateMaintenanceCommand(userId, id, body.Rating, body.Feedback));
        return await DispatchOrBadRequest(result);
    }

    // ==================================================================================
    // BUILDING ADMIN
    // ==================================================================================

    /// <summary>Список заявок по зданию с опциональными фильтрами (для админа здания).</summary>
    [HttpGet("building/{buildingId}")]
    public async Task<IActionResult> GetBuilding(
        Guid buildingId,
        [FromQuery] MaintenanceStatus? status,
        [FromQuery] MaintenancePriority? priority)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(
            new GetBuildingMaintenanceRequestsQuery(userId, buildingId, status, priority));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Админ подтверждает заявку (Reported → Acknowledged).</summary>
    [HttpPost("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new AcknowledgeMaintenanceCommand(userId, id));
        return await DispatchOrBadRequest(result);
    }

    /// <summary>Админ назначает подрядчика.</summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignBody body)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new AssignMaintenanceCommand(userId, id, body.StaffAssignmentId));
        return await DispatchOrBadRequest(result);
    }

    // ==================================================================================
    // CONTRACTOR / CLEANER
    // ==================================================================================

    /// <summary>Свободные задачи, которые подрядчик может взять.</summary>
    [HttpGet("work/available")]
    public async Task<IActionResult> GetAvailable()
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new GetAvailableWorkQuery(userId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Задачи, назначенные на подрядчика (в работе или ожидающие).</summary>
    [HttpGet("work/my")]
    public async Task<IActionResult> GetMyWork()
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new GetMyAssignedWorkQuery(userId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Подрядчик берёт задачу в работу (Assigned → InProgress).</summary>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new StartMaintenanceCommand(userId, id));
        return await DispatchOrBadRequest(result);
    }

    /// <summary>Подрядчик закрывает задачу с фото и заметкой.</summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteBody body)
    {
        var userId = RequireUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new CompleteMaintenanceCommand(
            userId, id, body.CompletionNotes, body.CompletionPhotoUrl));
        return await DispatchOrBadRequest(result);
    }

    // ==================================================================================
    // PRIVATE HELPERS
    // ==================================================================================

    private string? RequireUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Стандартная обработка workflow-результата: если успех — рассылаем SignalR-пуши,
    /// возвращаем 200. Иначе — 400 с текстом ошибки.
    /// </summary>
    private async Task<IActionResult> DispatchOrBadRequest(Result<MaintenanceActionResult> result)
    {
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });

        var r = result.Value!;

        // Пуш автору заявки — обновить свой список.
        await _hub.Clients.Group($"user_{r.ReporterUserId}")
            .SendAsync("MaintenanceStatusChanged", new
            {
                maintenanceId = r.MaintenanceId,
                status = r.NewStatus.ToString()
            });

        // Пуш подрядчику (если есть назначение) — обновить свой список задач.
        if (!string.IsNullOrEmpty(r.AssignedStaffUserId))
        {
            await _hub.Clients.Group($"user_{r.AssignedStaffUserId}")
                .SendAsync("MaintenanceAssignmentChanged", new
                {
                    maintenanceId = r.MaintenanceId,
                    status = r.NewStatus.ToString()
                });
        }

        // Пуш админам здания — у них тоже обновляется дашборд.
        await _hub.Clients.Group($"building_admin_{r.BuildingId}")
            .SendAsync("BuildingMaintenanceChanged", new
            {
                maintenanceId = r.MaintenanceId,
                status = r.NewStatus.ToString()
            });

        return Ok(new { maintenanceId = r.MaintenanceId, status = r.NewStatus.ToString() });
    }

    // ==================================================================================
    // REQUEST BODY TYPES
    // ==================================================================================
    public record CancelBody(string? Reason);
    public record RateBody(int Rating, string? Feedback);
    public record AssignBody(Guid StaffAssignmentId);
    public record CompleteBody(string? CompletionNotes, string? CompletionPhotoUrl);
}