using System.Security.Claims;
using CoLivingApp.Application.Features.Onboarding.Commands.CreateStaff;
using CoLivingApp.Application.Features.Onboarding.Commands.CreateTenant;
using CoLivingApp.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

/// <summary>
/// Эндпоинты онбординга: админ создаёт аккаунты жильцов и сотрудников при чек-ине.
/// 
/// Глобальная авторизация: только Admin/SuperAdmin (по User.Role в JWT).
/// Точечная авторизация по зданию (StaffAssignment) — внутри handler'ов.
/// 
/// AdminUserId намеренно НЕ берётся из тела запроса — только из JWT,
/// чтобы клиент не мог подменить личность инициатора.
/// </summary>
[ApiController]
[Route("api/admin/onboarding")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator) => _mediator = mediator;

    // === Request DTOs ===
    // AdminUserId НЕ в теле — берётся из JWT.
    public record CreateTenantRequest(
        Guid BuildingId,
        string Email,
        string Name,
        Guid RoomId);

    public record CreateStaffRequest(
        Guid BuildingId,
        string Email,
        string Name,
        StaffRole Role,
        ContractorType? Specialization);

    /// <summary>
    /// POST /api/admin/onboarding/tenants
    /// Создаёт аккаунт жильца и привязывает его к комнате.
    /// Возвращает временный пароль в plaintext один раз — админ обязан передать его жильцу.
    /// </summary>
    [HttpPost("tenants")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest req)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId)) return Unauthorized();

        var command = new CreateTenantUserCommand(
            adminUserId, req.BuildingId, req.Email, req.Name, req.RoomId);

        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// POST /api/admin/onboarding/staff
    /// Создаёт аккаунт сотрудника и StaffAssignment в указанном здании.
    /// </summary>
    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest req)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId)) return Unauthorized();

        var command = new CreateStaffUserCommand(
            adminUserId, req.BuildingId, req.Email, req.Name, req.Role, req.Specialization);

        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
}