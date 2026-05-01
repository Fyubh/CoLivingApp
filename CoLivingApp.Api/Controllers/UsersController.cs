// Файл: CoLivingApp.Api/Controllers/UsersController.cs

using System.Security.Claims;
using CoLivingApp.Application.Features.Users.Commands.CreateUser;
using CoLivingApp.Application.Features.Users.Commands.Auth;
using CoLivingApp.Application.Features.Inventory.Commands.RemoveItem;
using CoLivingApp.Application.Features.Inventory.Commands.MoveToCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Регистрация/Синхронизация пользователя в БД.
    /// (Legacy: используется при синхронизации с внешним провайдером — оставлено как есть.)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { userId = result.Value });
    }

    [HttpPut("{itemId}/cart")]
    public async Task<IActionResult> MoveToCart(Guid itemId, [FromBody] MoveItemToCartCommand command)
    {
        if (itemId != command.ItemId) return BadRequest();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpDelete("{itemId}")]
    public async Task<IActionResult> Remove(Guid itemId, [FromBody] RemoveItemCommand command)
    {
        if (itemId != command.ItemId) return BadRequest();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    /// <summary>
    /// POST /api/Users/change-password
    /// Принимает { oldPassword, newPassword }. UserId берётся из JWT.
    /// Возвращает { token, mustChangePassword: false } — НОВЫЙ JWT,
    /// который фронт должен немедленно сохранить вместо старого.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var command = new ChangePasswordCommand(userId, req.OldPassword, req.NewPassword);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { token = result.Value!.Token, mustChangePassword = result.Value.MustChangePassword })
            : BadRequest(new { error = result.Error });
    }

    // Body-DTO без UserId — UserId берётся из JWT, чтобы клиент не мог
    // подменить чужой ID.
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
}