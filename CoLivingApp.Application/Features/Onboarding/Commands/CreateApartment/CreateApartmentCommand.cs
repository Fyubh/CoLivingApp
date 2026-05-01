using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Commands.CreateApartment;

/// <summary>
/// Создание квартиры с её комнатами одной транзакцией.
///
/// Фронт сам формирует список Rooms по шаблону:
///  - Studio: [{ Number="A", Type=Studio, MaxOccupancy=1 }]
///  - Twin:   [{ Number="A", Type=Double, MaxOccupancy=2 }]
///  - 4BR:    [{ Number="A"|"B"|"C"|"D", Type=Single, MaxOccupancy=1 } x4]
/// </summary>
public record RoomTemplate(
    string Number,
    RoomType Type,
    int MaxOccupancy,
    decimal? SquareMeters,
    decimal? MonthlyRent);

public record CreateApartmentCommand(
    string AdminUserId,
    Guid BuildingId,
    Guid FloorId,
    string UnitNumber,
    string? Name,
    List<RoomTemplate> Rooms
) : IRequest<Result<Guid>>;

public class CreateApartmentCommandHandler
    : IRequestHandler<CreateApartmentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateApartmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateApartmentCommand request, CancellationToken ct)
    {
        // === 1. Базовая валидация ===
        if (string.IsNullOrWhiteSpace(request.UnitNumber))
            return Result<Guid>.Failure("Номер юнита обязателен.");

        if (request.Rooms == null || request.Rooms.Count == 0)
            return Result<Guid>.Failure("В квартире должна быть хотя бы одна комната.");

        if (request.Rooms.Count > 10)
            return Result<Guid>.Failure("Слишком много комнат в одной квартире.");

        // Уникальность Number в пределах списка комнат.
        var duplicateNumber = request.Rooms
            .GroupBy(r => r.Number, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateNumber != null)
            return Result<Guid>.Failure(
                $"Дублирующийся номер комнаты: {duplicateNumber.Key}.");

        foreach (var rt in request.Rooms)
        {
            if (string.IsNullOrWhiteSpace(rt.Number))
                return Result<Guid>.Failure("У каждой комнаты должен быть номер.");
            if (rt.MaxOccupancy < 1 || rt.MaxOccupancy > 6)
                return Result<Guid>.Failure(
                    "MaxOccupancy должен быть от 1 до 6.");
        }

        // === 2. Авторизация ===
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<Guid>.Failure(
                "У вас нет прав администратора в этом здании.");

        // === 3. Этаж существует и принадлежит этому зданию ===
        var floor = await _context.Floors
            .FirstOrDefaultAsync(f => f.Id == request.FloorId, ct);
        if (floor == null)
            return Result<Guid>.Failure("Этаж не найден.");
        if (floor.BuildingId != request.BuildingId)
            return Result<Guid>.Failure("Этаж не принадлежит этому зданию.");

        // === 4. UnitNumber уникален в пределах здания ===
        var unitTrimmed = request.UnitNumber.Trim();
        var unitTaken = await _context.Apartments.AnyAsync(a =>
            a.BuildingId == request.BuildingId
            && a.UnitNumber == unitTrimmed, ct);
        if (unitTaken)
            return Result<Guid>.Failure(
                $"Юнит {unitTrimmed} уже существует в этом здании.");

        // === 5. Создание ===
        var apartment = new Apartment
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(request.Name)
                ? $"Unit {unitTrimmed}"
                : request.Name.Trim(),
            UnitNumber = unitTrimmed,
            FloorId = floor.Id,
            BuildingId = request.BuildingId,
            InviteCode = string.Empty // B2B mode — InviteCode не используется
        };

        foreach (var rt in request.Rooms)
        {
            apartment.Rooms.Add(new Room
            {
                Id = Guid.NewGuid(),
                Number = rt.Number.Trim(),
                Type = rt.Type,
                Status = RoomStatus.Available,
                MaxOccupancy = rt.MaxOccupancy,
                SquareMeters = rt.SquareMeters,
                MonthlyRent = rt.MonthlyRent,
                CreatedById = request.AdminUserId
            });
        }

        _context.Apartments.Add(apartment);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(apartment.Id);
    }
}