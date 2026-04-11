// Файл: CoLivingApp.Api/Controllers/UsersController.cs
using CoLivingApp.Application.Features.Users.Commands.CreateUser;
using CoLivingApp.Application.Features.Inventory.Commands.RemoveItem;
using CoLivingApp.Application.Features.Inventory.Commands.MoveToCart;
using MediatR;
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
}