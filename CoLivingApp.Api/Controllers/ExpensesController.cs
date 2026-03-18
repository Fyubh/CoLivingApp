// Файл: CoLivingApp.Api/Controllers/ExpensesController.cs
using CoLivingApp.Application.Features.Expenses.Commands.CreateExpense;
using CoLivingApp.Application.Features.Expenses.Queries.GetBalance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Добавить новый расход и разделить его на всех жильцов.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(new { expenseId = result.Value }) : BadRequest(result.Error);
    }
    // Добавь в CoLivingApp.Api/Controllers/ExpensesController.cs

    [HttpGet("balance/{apartmentId}")]
    public async Task<IActionResult> GetBalance(Guid apartmentId)
    {
        var result = await _mediator.Send(new GetBalanceQuery(apartmentId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}