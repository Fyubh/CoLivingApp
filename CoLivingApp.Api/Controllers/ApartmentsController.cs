// Файл: CoLivingApp.Api/Controllers/ApartmentsController.cs
using CoLivingApp.Application.Features.Apartments.Commands.CreateApartment;
using CoLivingApp.Application.Features.Apartments.Commands.JoinApartment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoLivingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    // Внедряем интерфейс MediatR. 
    // Контроллеру вообще не нужно знать, как работает база данных!
    public ApartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Создает новую квартиру и генерирует инвайт-код.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateApartment([FromBody] CreateApartmentCommand command)
    {
        // Отправляем команду в MediatR. Он сам найдет нужный Handler.
        var result = await _mediator.Send(command);

        // Обрабатываем наш красивый паттерн Result
        if (!result.IsSuccess)
        {
            // Если ошибка бизнес-логики (например, юзер не найден)
            return BadRequest(new { error = result.Error });
        }

        // Если всё отлично, возвращаем HTTP 200 OK и ID новой квартиры
        return Ok(new { apartmentId = result.Value });
    }
    [HttpPost("join")]
    public async Task<IActionResult> JoinApartment([FromBody] JoinApartmentCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { apartmentId = result.Value });
    }
}