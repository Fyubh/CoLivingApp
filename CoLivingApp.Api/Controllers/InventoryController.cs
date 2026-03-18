// Файл: CoLivingApp.Api/Controllers/InventoryController.cs
using CoLivingApp.Application.Features.Inventory.Commands.CreateItem;
using CoLivingApp.Application.Features.Inventory.Queries.GetItems;
using CoLivingApp.Application.Features.Shopping.Commands.Checkout;
using CoLivingApp.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{apartmentId}")]
    public async Task<IActionResult> GetItems(Guid apartmentId, [FromQuery] ItemStatus status)
    {
        var result = await _mediator.Send(new GetItemsQuery(apartmentId, status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { expenseId = result.Value }) : BadRequest(new { error = result.Error });
    }
}