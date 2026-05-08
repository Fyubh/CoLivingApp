using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoLivingApp.Infrastructure.Seeders;

/// <summary>
/// Засевает демо-данные для The Fizz Prague + тестовых staff-пользователей.
/// 
/// После обновления: добавлено создание 3 staff-users и привязка через StaffAssignment.
/// Это нужно, чтобы в Maintenance-сценарии было кому назначать заявки.
/// </summary>
public static class 
    TheFizzPragueSeeder
{
    private const string OperatorSlug = "the-fizz";
    private const string BuildingName = "The Fizz Prague";

    // Тестовые логины staff — пароль для всех: "fizz123!" (хэш ниже подставляется BCrypt на лету)
    private const string DefaultStaffPassword = "fizz123!";

    // SuperAdmin — единственный пользователь, который может создавать здания.
    // Создаётся всегда, независимо от того, засеяно ли демо-здание The Fizz Prague.
    private const string SuperAdminEmail = "owner@coliving.test";
    private const string SuperAdminPassword = "owner123!";
    private const string SuperAdminName = "Platform Owner";

    // Демо-жилец для iOS-клиента: создаётся с MustChangePassword=true,
    // чтобы можно было проверить весь онбординг (Login → ChangePassword → Home).
    // Привязывается к конкретной студии в The Fizz Prague — solo-режим,
    // соседей нет. Для проверки shared-режима понадобится отдельный seed.
    private const string ResidentEmail = "ivan.resident@fizz.test";
    private const string ResidentName = "Ivan Petrov";
    private const string ResidentPassword = "fizz123!";
    private const string ResidentApartmentUnit = "0102";

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await EnsureSuperAdminSeeded(db, logger, ct);

        var existing = await db.Operators
            .FirstOrDefaultAsync(o => o.Slug == OperatorSlug, ct);

        if (existing != null)
        {
            // Идемпотентность для Building-слоя уже есть.
            // Проверим отдельно, есть ли staff-users — если нет, досеем (на случай старой базы,
            // засеянной предыдущей версией сидера).
            await EnsureStaffSeeded(db, logger, ct);
            await EnsureResidentSeeded(db, logger, ct);
            return;
        }

        logger.LogInformation("TheFizzPragueSeeder: seeding demo data for {Building}…", BuildingName);

        // 1. Operator
        var op = new Operator
        {
            Name = "The Fizz Group GmbH",
            Slug = OperatorSlug,
            ContactEmail = "billing@the-fizz.com",
            BrandPrimaryColor = "#FF6B35",
            IsActive = true
        };
        db.Operators.Add(op);

        // 2. Building
        var building = new Building
        {
            OperatorId = op.Id,
            Name = BuildingName,
            AddressLine = "Jateční 1530/37",
            City = "Prague",
            Country = "CZ",
            PostalCode = "170 00",
            Latitude = 50.1073m,
            Longitude = 14.4478m,
            TimeZone = "Europe/Prague",
            TotalFloors = 3,
            TotalApartments = 15,
            IsActive = true
        };
        db.Buildings.Add(building);

        // 3. Floors
        var floors = new[]
        {
            new Floor { BuildingId = building.Id, Number = 1, Name = "Ground Floor", HasSharedKitchen = true, HasSharedLounge = true, HasLaundry = true },
            new Floor { BuildingId = building.Id, Number = 2, Name = "Second Floor", HasSharedKitchen = true },
            new Floor { BuildingId = building.Id, Number = 3, Name = "Third Floor",  HasSharedKitchen = true }
        };
        db.Floors.AddRange(floors);

        // 4. Apartments + Rooms
        foreach (var floor in floors)
            SeedApartmentsForFloor(db, floor, building.Id);

        // 5. Staff
        SeedStaff(db, building.Id);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("TheFizzPragueSeeder: full seed done ({Apartments} apartments, 3 staff).",
            floors.Length * 5);

        await EnsureResidentSeeded(db, logger, ct);
    }

    /// <summary>
    /// Гарантирует, что в системе существует ровно один SuperAdmin.
    /// SuperAdmin не привязан к зданию — он создаёт здания, а потом получает
    /// StaffAssignment(BuildingAdmin) автоматически из CreateBuildingCommand.
    /// </summary>
    private static async Task EnsureSuperAdminSeeded(ApplicationDbContext db, ILogger logger, CancellationToken ct)
    {
        var emailLower = SuperAdminEmail.ToLowerInvariant();
        var existing = await db.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);

        if (existing != null)
        {
            // На случай старой БД, где этот юзер был создан с другой ролью —
            // подтянем его к актуальной SuperAdmin-роли.
            if (existing.Role != UserRole.SuperAdmin)
            {
                existing.Role = UserRole.SuperAdmin;
                existing.AccessLevel = 10;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("TheFizzPragueSeeder: promoted existing user {Email} to SuperAdmin.", SuperAdminEmail);
            }
            return;
        }

        var owner = new User
        {
            Email = emailLower,
            Name = SuperAdminName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(SuperAdminPassword),
            Role = UserRole.SuperAdmin,
            AccessLevel = 10,
            KarmaScore = 100,
            MustChangePassword = false
        };
        db.Users.Add(owner);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("TheFizzPragueSeeder: created SuperAdmin {Email} (password={Password}).",
            SuperAdminEmail, SuperAdminPassword);
    }

    private static async Task EnsureStaffSeeded(ApplicationDbContext db, ILogger logger, CancellationToken ct)
    {
        var building = await db.Buildings
            .FirstOrDefaultAsync(b => b.Name == BuildingName, ct);
        if (building == null) return;

        var hasStaff = await db.StaffAssignments
            .AnyAsync(s => s.BuildingId == building.Id, ct);

        if (hasStaff)
        {
            logger.LogInformation("TheFizzPragueSeeder: staff already seeded, skipping.");
            return;
        }

        logger.LogInformation("TheFizzPragueSeeder: back-filling staff for existing building…");
        SeedStaff(db, building.Id);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Гарантирует, что в The Fizz Prague живёт демо-жилец, через которого
    /// можно прогнать весь онбординг iOS-клиента (Login → ChangePassword → Home).
    /// Создаёт User(Tenant) с MustChangePassword=true (если ещё нет) и привязывает
    /// его через ApartmentMember к Studio 0102. Solo-режим — соседей нет.
    /// </summary>
    private static async Task EnsureResidentSeeded(ApplicationDbContext db, ILogger logger, CancellationToken ct)
    {
        var building = await db.Buildings
            .FirstOrDefaultAsync(b => b.Name == BuildingName, ct);
        if (building == null) return;

        var emailLower = ResidentEmail.ToLowerInvariant();
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);

        var createdUser = user == null;
        if (user == null)
        {
            user = new User
            {
                Email = emailLower,
                Name = ResidentName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(ResidentPassword),
                Role = UserRole.Tenant,
                AccessLevel = 1,
                KarmaScore = 100,
                MustChangePassword = true
            };
            db.Users.Add(user);
        }

        var hasMembership = await db.ApartmentMembers
            .AnyAsync(m => m.UserId == user.Id && m.IsActive, ct);

        if (!hasMembership)
        {
            var apartment = await db.Apartments
                .Where(a => a.BuildingId == building.Id && a.UnitNumber == ResidentApartmentUnit)
                .Include(a => a.Rooms)
                .FirstOrDefaultAsync(ct);

            if (apartment != null)
            {
                var room = apartment.Rooms.OrderBy(r => r.Number).FirstOrDefault();
                db.ApartmentMembers.Add(new ApartmentMember
                {
                    UserId = user.Id,
                    ApartmentId = apartment.Id,
                    RoomId = room?.Id,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow
                });
            }
            else
            {
                logger.LogWarning(
                    "TheFizzPragueSeeder: target apartment {Unit} not found, skipping resident membership.",
                    ResidentApartmentUnit);
            }
        }

        if (createdUser || !hasMembership)
        {
            await db.SaveChangesAsync(ct);
        }

        if (createdUser)
        {
            logger.LogInformation(
                "TheFizzPragueSeeder: created Tenant {Email} (password={Password}, must change on first login).",
                ResidentEmail, ResidentPassword);
        }
    }

    private static void SeedApartmentsForFloor(ApplicationDbContext db, Floor floor, Guid buildingId)
    {
        var prefix = $"{floor.Number:D2}";

        for (int i = 1; i <= 3; i++)
        {
            var studio = new Apartment
            {
                Name = $"Studio {prefix}{i:D2}",
                UnitNumber = $"{prefix}{i:D2}",
                InviteCode = string.Empty,
                FloorId = floor.Id,
                BuildingId = buildingId
            };
            studio.Rooms.Add(new Room
            {
                Number = "A",
                Type = RoomType.Studio,
                Status = RoomStatus.Available,
                SquareMeters = 22.5m,
                MonthlyRent = 850m,
                MaxOccupancy = 1
            });
            db.Apartments.Add(studio);
        }

        var twin = new Apartment
        {
            Name = $"Twin Room {prefix}04",
            UnitNumber = $"{prefix}04",
            InviteCode = string.Empty,
            FloorId = floor.Id,
            BuildingId = buildingId
        };
        twin.Rooms.Add(new Room
        {
            Number = "A",
            Type = RoomType.Double,
            Status = RoomStatus.Available,
            SquareMeters = 26m,
            MonthlyRent = 550m,
            MaxOccupancy = 2
        });
        db.Apartments.Add(twin);

        var sharedFlat = new Apartment
        {
            Name = $"Shared Flat {prefix}05",
            UnitNumber = $"{prefix}05",
            InviteCode = string.Empty,
            FloorId = floor.Id,
            BuildingId = buildingId
        };
        foreach (var letter in new[] { "A", "B", "C", "D" })
        {
            sharedFlat.Rooms.Add(new Room
            {
                Number = letter,
                Type = RoomType.Single,
                Status = RoomStatus.Available,
                SquareMeters = 14.5m,
                MonthlyRent = 620m,
                MaxOccupancy = 1
            });
        }
        db.Apartments.Add(sharedFlat);
    }

    /// <summary>
    /// Создаёт тестовых сотрудников для The Fizz Prague:
    /// - Maria Novakova — BuildingAdmin (логин: maria.admin@fizz.test)
    /// - Pavel Svoboda — Contractor / Plumbing (логин: pavel.plumber@fizz.test)
    /// - Anna Dvořáková — Contractor / Cleaning (логин: anna.cleaner@fizz.test)
    /// Пароль у всех: fizz123!
    /// </summary>
    private static void SeedStaff(ApplicationDbContext db, Guid buildingId)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(DefaultStaffPassword);

        var admin = new User
        {
            Email = "maria.admin@fizz.test",
            Name = "Maria Novakova",
            PasswordHash = hashedPassword,
            Role = UserRole.Admin,
            AccessLevel = 5,
            KarmaScore = 100
        };
        var plumber = new User
        {
            Email = "pavel.plumber@fizz.test",
            Name = "Pavel Svoboda",
            PasswordHash = hashedPassword,
            Role = UserRole.Staff,
            AccessLevel = 2,
            KarmaScore = 100
        };
        var cleaner = new User
        {
            Email = "anna.cleaner@fizz.test",
            Name = "Anna Dvořáková",
            PasswordHash = hashedPassword,
            Role = UserRole.Staff,
            AccessLevel = 1,
            KarmaScore = 100
        };

        db.Users.AddRange(admin, plumber, cleaner);

        db.StaffAssignments.AddRange(
            new StaffAssignment
            {
                UserId = admin.Id,
                BuildingId = buildingId,
                Role = StaffRole.BuildingAdmin,
                IsActive = true,
                IsOnShift = true,
                ShiftStartedAt = DateTime.UtcNow
            },
            new StaffAssignment
            {
                UserId = plumber.Id,
                BuildingId = buildingId,
                Role = StaffRole.Contractor,
                Specialization = ContractorType.Plumbing,
                IsActive = true,
                IsOnShift = false
            },
            new StaffAssignment
            {
                UserId = cleaner.Id,
                BuildingId = buildingId,
                Role = StaffRole.Cleaner,
                IsActive = true,
                IsOnShift = false
            }
        );
    }
}