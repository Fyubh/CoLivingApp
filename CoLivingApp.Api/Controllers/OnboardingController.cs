using System.Security.Claims;
using CoLivingApp.Application.Features.Onboarding.Commands.CreateApartment;
using CoLivingApp.Application.Features.Onboarding.Commands.CreateStaff;
using CoLivingApp.Application.Features.Onboarding.Commands.CreateTenant;
using CoLivingApp.Application.Features.Onboarding.Queries.GetAvailableRooms;
using CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingApartments;
using CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingFloors;
using CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingResidents;
using CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingStaff;
using CoLivingApp.Application.Features.Onboarding.Queries.GetMyBuildings;
using CoLivingApp.Application.Features.Onboarding.Commands.CreateBuilding;
using CoLivingApp.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/admin/onboarding")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;
    public OnboardingController(IMediator mediator) => _mediator = mediator;

    private string? CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ========== СОЗДАНИЕ ЮЗЕРОВ ==========

    public record CreateTenantRequest(Guid BuildingId, string Email, string Name, Guid RoomId);

    [HttpPost("tenants")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest req)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new CreateTenantUserCommand(
            adminId, req.BuildingId, req.Email, req.Name, req.RoomId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    public record CreateStaffRequest(Guid BuildingId, string Email, string Name,
        StaffRole Role, ContractorType? Specialization);

    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest req)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new CreateStaffUserCommand(
            adminId, req.BuildingId, req.Email, req.Name, req.Role, req.Specialization));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ========== СОЗДАНИЕ КВАРТИРЫ ==========

    public record CreateApartmentRequest(Guid BuildingId, Guid FloorId,
        string UnitNumber, string? Name, List<RoomTemplate> Rooms);

    [HttpPost("apartments")]
    public async Task<IActionResult> CreateApartment([FromBody] CreateApartmentRequest req)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new CreateApartmentCommand(
            adminId, req.BuildingId, req.FloorId, req.UnitNumber, req.Name, req.Rooms));
        return result.IsSuccess ? Ok(new { apartmentId = result.Value }) : BadRequest(new { error = result.Error });
    }

    // ========== СПРАВОЧНИКИ ==========

    [HttpGet("buildings")]
    public async Task<IActionResult> GetMyBuildings()
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new GetMyBuildingsQuery(adminId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("buildings/{buildingId:guid}/floors")]
    public async Task<IActionResult> GetFloors(Guid buildingId)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new GetBuildingFloorsQuery(adminId, buildingId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("buildings/{buildingId:guid}/available-rooms")]
    public async Task<IActionResult> GetAvailableRooms(Guid buildingId)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new GetAvailableRoomsQuery(adminId, buildingId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("buildings/{buildingId:guid}/staff")]
    public async Task<IActionResult> GetStaff(Guid buildingId)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new GetBuildingStaffQuery(adminId, buildingId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("buildings/{buildingId:guid}/residents")]
    public async Task<IActionResult> GetResidents(Guid buildingId)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new GetBuildingResidentsQuery(adminId, buildingId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Все квартиры здания со всеми комнатами и счётчиками жильцов.</summary>
    [HttpGet("buildings/{buildingId:guid}/apartments")]
    public async Task<IActionResult> GetApartments(Guid buildingId)
    {
        var adminId = CurrentUserId(); if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        var result = await _mediator.Send(new GetBuildingApartmentsQuery(adminId, buildingId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
    
    // ========== СОЗДАНИЕ ЗДАНИЯ (только SuperAdmin) ==========

    public record CreateBuildingRequest(
        string Name,
        string AddressLine,
        string City,
        string Country,
        string? PostalCode,
        string? TimeZone,
        int TotalFloors,
        Guid? OperatorId
    );

    [HttpPost("buildings")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingRequest req)
    {
        var userId = CurrentUserId(); if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _mediator.Send(new CreateBuildingCommand(
            userId, req.Name, req.AddressLine, req.City, req.Country,
            req.PostalCode, req.TimeZone, req.TotalFloors, req.OperatorId));
        return result.IsSuccess
            ? Ok(new { buildingId = result.Value })
            : BadRequest(new { error = result.Error });
    }
}
