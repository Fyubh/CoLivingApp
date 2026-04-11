using CoLivingApp.Application.Features.Apartments.Commands.CreateApartment;
using CoLivingApp.Application.Features.Apartments.Commands.JoinApartment;
using CoLivingApp.Application.Features.Apartments.Queries.GetApartment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoLivingApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApartmentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateApartmentCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var secureCommand = command with { CreatorUserId = userId! };

        var result = await _mediator.Send(secureCommand);
        return result.IsSuccess ? Ok(new { apartmentId = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinApartmentCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var secureCommand = command with { UserId = userId! };

        var result = await _mediator.Send(secureCommand);
        return result.IsSuccess ? Ok(new { apartmentId = result.Value }) : BadRequest(new { error = result.Error });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetApartmentQuery(id));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}