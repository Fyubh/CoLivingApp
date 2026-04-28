using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Common;
using CoLivingApp.Application.Features.Onboarding.Shared;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Commands.CreateTenant;

/// <summary>
/// Админ создаёт аккаунт жильца и привязывает его к конкретной комнате.
///
/// Workflow:
///  1. Авторизация: AdminUserId должен быть BuildingAdmin в BuildingId.
///  2. Email-уникальность (case-insensitive).
///  3. Комната существует и в этом же здании.
///  4. В комнате есть свободное место (count active members &lt; Room.MaxOccupancy).
///  5. Создаётся User (Role=Tenant, MustChangePassword=true) с BCrypt-хэшем temp-пароля.
///  6. Создаётся ApartmentMember (UserId, ApartmentId, RoomId, IsActive=true).
///  7. В ответе админу возвращается plaintext temp-пароль ОДИН РАЗ.
/// </summary>
public record CreateTenantUserCommand(
    string AdminUserId,
    Guid BuildingId,
    string Email,
    string Name,
    Guid RoomId
) : IRequest<Result<CreatedUserDto>>;

public class CreateTenantUserCommandHandler
    : IRequestHandler<CreateTenantUserCommand, Result<CreatedUserDto>>
{
    private readonly IApplicationDbContext _context;
    public CreateTenantUserCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<CreatedUserDto>> Handle(
        CreateTenantUserCommand request, CancellationToken ct)
    {
        // === 1. Базовая валидация полей ===
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<CreatedUserDto>.Failure("Email обязателен.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<CreatedUserDto>.Failure("Имя обязательно.");

        // Нормализуем email: трим и lower — чтобы не плодить дубли с разным регистром.
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var name = request.Name.Trim();

        if (!normalizedEmail.Contains('@'))
            return Result<CreatedUserDto>.Failure("Некорректный email.");

        // === 2. Авторизация ===
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<CreatedUserDto>.Failure(
                "У вас нет прав администратора в этом здании.");

        // === 3. Email-уникальность ===
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct);
        if (emailTaken)
            return Result<CreatedUserDto>.Failure(
                $"Пользователь с email {normalizedEmail} уже существует.");

        // === 4. Комната существует и в этом здании ===
        var room = await _context.Rooms
            .Include(r => r.Apartment)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, ct);
        if (room == null)
            return Result<CreatedUserDto>.Failure("Комната не найдена.");

        if (room.Apartment == null || room.Apartment.BuildingId != request.BuildingId)
            return Result<CreatedUserDto>.Failure(
                "Комната не принадлежит этому зданию.");

        // === 5. В комнате есть свободное место ===
        var activeOccupants = await _context.ApartmentMembers
            .CountAsync(m => m.RoomId == request.RoomId && m.IsActive, ct);

        if (activeOccupants >= room.MaxOccupancy)
            return Result<CreatedUserDto>.Failure(
                $"Комната занята: {activeOccupants}/{room.MaxOccupancy} мест занято.");

        // === 6. Генерим пароль и создаём сущности ===
        var tempPassword = TempPasswordGenerator.Generate();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = normalizedEmail,
            Name = name,
            PasswordHash = passwordHash,
            Role = UserRole.Tenant,
            AccessLevel = 1,
            KarmaScore = 100,
            MustChangePassword = true
        };
        _context.Users.Add(user);

        var member = new ApartmentMember
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ApartmentId = room.ApartmentId,
            RoomId = room.Id,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
        _context.ApartmentMembers.Add(member);

        await _context.SaveChangesAsync(ct);

        return Result<CreatedUserDto>.Success(new CreatedUserDto(
            user.Id, user.Email, user.Name, tempPassword));
    }
}