using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Commands.CreateBuilding;

/// <summary>
/// Создание нового здания. Доступно ТОЛЬКО SuperAdmin.
/// OperatorId опциональный: если не передан — берём первого активного оператора в системе
/// (для MVP с одним SaaS-клиентом). В будущем добавим выбор оператора в UI.
/// </summary>
public record CreateBuildingCommand(
    string RequestingUserId,
    string Name,
    string AddressLine,
    string City,
    string Country,        // ISO-2: "CZ", "DE"
    string? PostalCode,
    string? TimeZone,
    int TotalFloors,
    Guid? OperatorId
) : IRequest<Result<Guid>>;

public class CreateBuildingCommandHandler
    : IRequestHandler<CreateBuildingCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateBuildingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateBuildingCommand request, CancellationToken ct)
    {
        // 1. Авторизация: только SuperAdmin может создавать здания.
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.RequestingUserId, ct);

        if (user == null)
            return Result<Guid>.Failure("Пользователь не найден.");

        if (user.Role != UserRole.SuperAdmin)
            return Result<Guid>.Failure("Только SuperAdmin может создавать здания.");

        // 2. Базовая валидация полей.
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Guid>.Failure("Название обязательно.");
        if (string.IsNullOrWhiteSpace(request.AddressLine))
            return Result<Guid>.Failure("Адрес обязателен.");
        if (string.IsNullOrWhiteSpace(request.City))
            return Result<Guid>.Failure("Город обязателен.");
        if (request.Country?.Length != 2)
            return Result<Guid>.Failure("Country должен быть ISO-2 кодом (CZ, DE).");
        if (request.TotalFloors < 1)
            return Result<Guid>.Failure("Этажей должно быть >= 1.");

        // 3. Определяем оператора. Если не передан — берём первого активного.
        Guid operatorId;
        if (request.OperatorId.HasValue)
        {
            var exists = await _context.Operators
                .AnyAsync(o => o.Id == request.OperatorId.Value && o.IsActive, ct);
            if (!exists)
                return Result<Guid>.Failure("Оператор не найден или неактивен.");
            operatorId = request.OperatorId.Value;
        }
        else
        {
            var firstOp = await _context.Operators
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .Select(o => (Guid?)o.Id)
                .FirstOrDefaultAsync(ct);

            if (firstOp == null)
                return Result<Guid>.Failure("В системе нет активных операторов. Создайте оператора сначала.");

            operatorId = firstOp.Value;
        }

        // 4. Создаём здание.
        var building = new Building
        {
            Id = Guid.NewGuid(),
            OperatorId = operatorId,
            Name = request.Name.Trim(),
            AddressLine = request.AddressLine.Trim(),
            City = request.City.Trim(),
            Country = request.Country.ToUpperInvariant(),
            PostalCode = request.PostalCode?.Trim(),
            TimeZone = request.TimeZone?.Trim(),
            TotalFloors = request.TotalFloors,
            TotalApartments = 0,
            IsActive = true
        };

        _context.Buildings.Add(building);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(building.Id);
    }
}