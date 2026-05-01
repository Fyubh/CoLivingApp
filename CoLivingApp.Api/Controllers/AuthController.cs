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

    // === Эндпоинт /api/Auth/register удалён. ===
    // В B2B-модели пользователи не регистрируются сами — их создаёт админ
    // при чек-ине через POST /api/admin/onboarding/tenants (или .../staff)
    // и выдаёт временный пароль. См. OnboardingController.

    /// <summary>
    /// POST /api/Auth/login
    /// Возвращает { token, mustChangePassword }.
    /// Если mustChangePassword == true — фронт обязан повести юзера на форму
    /// смены пароля и не давать пользоваться остальным функционалом до смены.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? Ok(new { token = result.Value!.Token, mustChangePassword = result.Value.MustChangePassword })
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