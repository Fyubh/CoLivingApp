using System.Security.Claims;
using CoLivingApp.Application.Features.Users.Commands.Auth;
using CoLivingApp.Application.Features.Apartments.Queries.GetMyApartmentContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    // === ВАЖНО: Эндпоинт /api/Auth/register удалён. ===
    // В B2B-модели пользователи не регистрируются сами — их создаёт админ
    // при чек-ине через POST /api/admin/onboarding/tenants (или .../staff)
    // и выдаёт временный пароль. См. OnboardingController.
    //
    // Если в будущем понадобится consumer-mode регистрация (две подруги
    // в съёмной квартире) — её нужно делать через отдельный invite-flow,
    // а не возвращать публичный register-endpoint.

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? Ok(new { token = result.Value })
            : Unauthorized(new { error = result.Error });
    }

    [HttpGet("my-context")]
    public async Task<IActionResult> GetMyContext()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new GetMyApartmentContextQuery(userId));
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
}