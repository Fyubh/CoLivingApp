using CoLivingApp.Application.Features.Expenses.Commands.CreateExpense;
using CoLivingApp.Application.Features.Expenses.Commands.SettleDebt;
using CoLivingApp.Application.Features.Expenses.Queries.GetBalance;
using CoLivingApp.Application.Features.Expenses.Queries.GetExpenses;
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
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<CoLivingHub> _hub;

    public ExpensesController(IMediator mediator, IHubContext<CoLivingHub> hub)
    {
        _mediator = mediator;
        _hub = hub;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(command with { PayerId = userId! });
        
        if (result.IsSuccess) {
            await _hub.Clients.Group(command.ApartmentId.ToString()).SendAsync("UpdateFinance");
            return Ok(new { expenseId = result.Value });
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpPost("settle")]
    public async Task<IActionResult> SettleDebt([FromBody] SettleDebtCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(command with { SenderId = userId! });

        if (result.IsSuccess) {
            await _hub.Clients.Group(command.ApartmentId.ToString()).SendAsync("UpdateFinance");
            return Ok(new { settlementId = result.Value });
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpGet("balance/{apartmentId}")]
    public async Task<IActionResult> GetBalance(Guid apartmentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Достаем ID
        var result = await _mediator.Send(new GetBalanceQuery(apartmentId, userId!)); // Передаем
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{apartmentId}")]
    public async Task<IActionResult> GetExpenses(Guid apartmentId)
    {
        var result = await _mediator.Send(new GetExpensesQuery(apartmentId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
    [HttpGet("settlements/{apartmentId}")]
    public async Task<IActionResult> GetSettlements(Guid apartmentId)
    {
        var result = await _mediator.Send(new CoLivingApp.Application.Features.Expenses.Queries.GetSettlements.GetSettlementsQuery(apartmentId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}