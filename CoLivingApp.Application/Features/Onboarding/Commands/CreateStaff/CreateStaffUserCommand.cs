using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Common;
using CoLivingApp.Application.Features.Onboarding.Shared;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Commands.CreateStaff;

/// <summary>
/// Админ создаёт аккаунт сотрудника (Cleaner / Contractor / Reception / BuildingAdmin / Security)
/// с привязкой через StaffAssignment к зданию.
///
/// Workflow:
///  1. Авторизация: AdminUserId должен быть BuildingAdmin в BuildingId.
///  2. Email-уникальность.
///  3. Валидация связки Role + Specialization:
///     - Contractor → Specialization обязателен.
///     - все остальные роли → Specialization должен быть null.
///  4. Создаётся User с глобальной ролью UserRole (mapped из StaffRole).
///  5. Создаётся StaffAssignment.
///  6. В ответе админу возвращается plaintext temp-пароль один раз.
///
/// Маппинг StaffRole → UserRole (глобальная роль для JWT и [Authorize]):
///   BuildingAdmin → UserRole.Admin
///   все остальные → UserRole.Staff
/// </summary>
public record CreateStaffUserCommand(
    string AdminUserId,
    Guid BuildingId,
    string Email,
    string Name,
    StaffRole Role,
    ContractorType? Specialization
) : IRequest<Result<CreatedUserDto>>;

public class CreateStaffUserCommandHandler
    : IRequestHandler<CreateStaffUserCommand, Result<CreatedUserDto>>
{
    private readonly IApplicationDbContext _context;
    public CreateStaffUserCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<CreatedUserDto>> Handle(
        CreateStaffUserCommand request, CancellationToken ct)
    {
        // === 1. Базовая валидация полей ===
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<CreatedUserDto>.Failure("Email обязателен.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<CreatedUserDto>.Failure("Имя обязательно.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var name = request.Name.Trim();

        if (!normalizedEmail.Contains('@'))
            return Result<CreatedUserDto>.Failure("Некорректный email.");

        // === 2. Валидация связки Role + Specialization ===
        if (request.Role == StaffRole.Contractor && request.Specialization == null)
            return Result<CreatedUserDto>.Failure(
                "Для подрядчика обязательно указать специализацию.");

        if (request.Role != StaffRole.Contractor && request.Specialization != null)
            return Result<CreatedUserDto>.Failure(
                "Специализация задаётся только для роли Contractor.");

        // === 3. Авторизация ===
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<CreatedUserDto>.Failure(
                "У вас нет прав администратора в этом здании.");

        // === 4. Здание существует ===
        var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == request.BuildingId, ct);
        if (!buildingExists)
            return Result<CreatedUserDto>.Failure("Здание не найдено.");

        // === 5. Email-уникальность ===
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct);
        if (emailTaken)
            return Result<CreatedUserDto>.Failure(
                $"Пользователь с email {normalizedEmail} уже существует.");

        // === 6. Маппинг StaffRole → UserRole ===
        var userRole = request.Role == StaffRole.BuildingAdmin
            ? UserRole.Admin
            : UserRole.Staff;

        // === 7. Создание сущностей ===
        var tempPassword = TempPasswordGenerator.Generate();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = normalizedEmail,
            Name = name,
            PasswordHash = passwordHash,
            Role = userRole,
            AccessLevel = userRole == UserRole.Admin ? 5 : 1,
            KarmaScore = 100,
            MustChangePassword = true
        };
        _context.Users.Add(user);

        var assignment = new StaffAssignment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BuildingId = request.BuildingId,
            Role = request.Role,
            Specialization = request.Specialization,
            IsActive = true,
            IsOnShift = false,
            CompletedTasksCount = 0,
            CreatedById = request.AdminUserId
        };
        _context.StaffAssignments.Add(assignment);

        await _context.SaveChangesAsync(ct);

        return Result<CreatedUserDto>.Success(new CreatedUserDto(
            user.Id, user.Email, user.Name, tempPassword));
    }
}