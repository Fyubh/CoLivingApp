using CoLivingApp.Application.Features.Inventory.Commands.CreateItem;
using CoLivingApp.Application.Features.Inventory.Commands.MoveToCart;
using CoLivingApp.Application.Features.Inventory.Commands.RemoveItem;
using CoLivingApp.Application.Features.Inventory.Commands.UpdateItem;
using CoLivingApp.Application.Features.Inventory.Queries.GetItems;
using CoLivingApp.Application.Features.Shopping.Commands.Checkout;
using CoLivingApp.Application.Features.Inventory.Commands.ConsumeItem;
using CoLivingApp.Domain.Enums;
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
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<CoLivingHub> _hub;

    public InventoryController(IMediator mediator, IHubContext<CoLivingHub> hub)
    {
        _mediator = mediator;
        _hub = hub;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(command with { UserId = userId! });

        if (result.IsSuccess)
        {
            await _hub.Clients.Group(command.ApartmentId.ToString()).SendAsync("UpdateInventory");
            return Ok(new { itemId = result.Value });
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpGet("{apartmentId}")]
    public async Task<IActionResult> GetItems(Guid apartmentId, [FromQuery] ItemStatus status)
    {
        var result = await _mediator.Send(new GetItemsQuery(apartmentId, status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // PATCH /api/Inventory/{itemId} — редактирование товара
    [HttpPatch("{itemId}")]
    public async Task<IActionResult> Update(Guid itemId, [FromBody] UpdateItemCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var secureCommand = command with { ItemId = itemId, UserId = userId! };
        var result = await _mediator.Send(secureCommand);

        if (result.IsSuccess)
        {
            await _hub.Clients.Group(command.ApartmentId.ToString()).SendAsync("UpdateInventory");
            return Ok();
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpPost("move-to-cart")]
    public async Task<IActionResult> MoveToCart([FromBody] MoveItemToCartCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(command with { UserId = userId! });

        if (result.IsSuccess)
        {
            await _hub.Clients.Group(command.ApartmentId.ToString()).SendAsync("UpdateInventory");
            return Ok();
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpDelete("{itemId}/{apartmentId}")]
    public async Task<IActionResult> Remove(Guid itemId, Guid apartmentId)
    {
        var result = await _mediator.Send(new RemoveItemCommand(itemId, apartmentId));
        if (result.IsSuccess)
        {
            await _hub.Clients.Group(apartmentId.ToString()).SendAsync("UpdateInventory");
            return Ok();
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(command with { PayerId = userId! });

        if (result.IsSuccess)
        {
            await _hub.Clients.Group(command.ApartmentId.ToString()).SendAsync("UpdateInventory");
            await _hub.Clients.Group(command.ApartmentId.ToString()).SendAsync("UpdateFinance");
            return Ok(new { expenseId = result.Value });
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpPost("consume")]
    public async Task<IActionResult> Consume([FromBody] ConsumeItemCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var secureCommand = command with { UserId = userId! };
        var result = await _mediator.Send(secureCommand);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}