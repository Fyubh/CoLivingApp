using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Commands.CreateMaintenanceRequest;

public class CreateMaintenanceRequestCommandHandler
    : IRequestHandler<CreateMaintenanceRequestCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateMaintenanceRequestCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateMaintenanceRequestCommand request, CancellationToken ct)
    {
        // 1. Валидация обязательных полей
        if (string.IsNullOrWhiteSpace(request.ReportedByUserId))
            return Result<Guid>.Failure("Не указан пользователь.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure("Заголовок обязателен.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Result<Guid>.Failure("Описание обязательно.");

        // 2. Разрешаем место заявки по каскаду: Room → Apartment → Building.
        //    По пути вытаскиваем Apartment/Building, чтобы заполнить денормализованные FK.
        Guid? resolvedBuildingId = null;
        Guid? resolvedApartmentId = request.ApartmentId;
        Guid? resolvedRoomId = null;

        if (request.RoomId.HasValue)
        {
            var room = await _context.Rooms
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.Id == request.RoomId.Value, ct);

            if (room == null)
                return Result<Guid>.Failure("Комната не найдена.");

            if (room.Apartment?.BuildingId == null)
                return Result<Guid>.Failure("Комната не привязана к зданию — заявка в consumer-mode не поддерживается.");

            resolvedRoomId = room.Id;
            resolvedApartmentId = room.ApartmentId;
            resolvedBuildingId = room.Apartment.BuildingId;
        }
        else if (request.ApartmentId.HasValue)
        {
            var apartment = await _context.Apartments
                .FirstOrDefaultAsync(a => a.Id == request.ApartmentId.Value, ct);

            if (apartment == null)
                return Result<Guid>.Failure("Квартира не найдена.");

            if (apartment.BuildingId == null)
                return Result<Guid>.Failure("Квартира не привязана к зданию — заявка в consumer-mode не поддерживается.");

            resolvedApartmentId = apartment.Id;
            resolvedBuildingId = apartment.BuildingId;
        }
        else if (request.BuildingId.HasValue)
        {
            var buildingExists = await _context.Buildings
                .AnyAsync(b => b.Id == request.BuildingId.Value, ct);

            if (!buildingExists)
                return Result<Guid>.Failure("Здание не найдено.");

            resolvedBuildingId = request.BuildingId.Value;
        }
        else
        {
            return Result<Guid>.Failure("Не указано место заявки (room/apartment/building).");
        }

        // 3. Авторизация: жилец должен быть членом квартиры, по которой подаёт заявку.
        //    Для заявок по общей зоне здания (без ApartmentId) — пропускаем, т.к. жилец просто
        //    в этом здании живёт (позже заменим на StaffAssignment/Tenancy-based check).
        if (resolvedApartmentId.HasValue)
        {
            var isMember = await _context.ApartmentMembers
                .AnyAsync(m => m.ApartmentId == resolvedApartmentId.Value
                               && m.UserId == request.ReportedByUserId
                               && m.IsActive, ct);

            if (!isMember)
                return Result<Guid>.Failure("Вы не являетесь жильцом этой квартиры.");
        }

        // 4. Создаём заявку
        var maintenance = new MaintenanceRequest
        {
            BuildingId = resolvedBuildingId!.Value,
            ApartmentId = resolvedApartmentId,
            RoomId = resolvedRoomId,
            ReportedByUserId = request.ReportedByUserId,
            Category = request.Category,
            Priority = request.Priority,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            PhotoUrl = request.PhotoUrl,
            CreatedById = request.ReportedByUserId
        };

        _context.MaintenanceRequests.Add(maintenance);
        await _context.SaveChangesAsync(ct);

        // В следующей сессии: SignalR-пуш админам здания через CoLivingHub.
        return Result<Guid>.Success(maintenance.Id);
    }
}