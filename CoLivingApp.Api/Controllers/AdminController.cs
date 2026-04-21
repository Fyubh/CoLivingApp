using CoLivingApp.Application.Features.Admin.Queries.GetTenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoLivingApp.Application.Features.Admin.Commands.ProcessIncident;
using CoLivingApp.Application.Features.Admin.Commands.ReportIncident;
using CoLivingApp.Application.Features.Admin.Queries.GetDashboard;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// Требуем, чтобы юзер был авторизован и имел роль Admin или SuperAdmin
[Authorize(Roles = "Admin,SuperAdmin")] 
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants([FromQuery] string? search, [FromQuery] bool sortByKarmaAsc = false)
    {
        var query = new GetTenantsQuery 
        { 
            SearchTerm = search, 
            SortByKarmaAscending = sortByKarmaAsc 
        };
        
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // Обработка инцидента админом (кнопки Approve/Reject)
    [HttpPost("incidents/process")]
    public async Task<IActionResult> ProcessIncident([FromBody] ProcessIncidentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    // ЗАГЛУШКА ДЛЯ ИИ (Webhook) - Этот эндпоинт открыт для внешних сервисов (без [Authorize])
    [AllowAnonymous] 
    [HttpPost("webhook/ai-incident")]
    public async Task<IActionResult> AiWebhookReport([FromBody] ReportAiIncidentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { IncidentId = result.Value }) : BadRequest(result.Error);
    }
}
