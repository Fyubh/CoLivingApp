// Файл: CoLivingApp.Application/Features/Apartments/Commands/CreateApartment/CreateApartmentCommandHandler.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Apartments.Commands.CreateApartment;

public class CreateApartmentCommandHandler : IRequestHandler<CreateApartmentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    // Внедряем нашу базу данных через интерфейс (Dependency Injection)
    public CreateApartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateApartmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Проверяем, существует ли пользователь в базе
        // (Обычно ID берется из токена авторизации, но мы пока сделаем явную проверку)
        var userExists = _context.Users.Any(u => u.Id == request.CreatorUserId);
        if (!userExists)
        {
            return Result<Guid>.Failure("Пользователь не найден.");
        }

        // 2. Генерируем уникальный инвайт-код (например, A7K9P2)
        var inviteCode = GenerateInviteCode();
        
        // Маленькая защита: проверяем, нет ли уже такого кода в базе (защита от коллизий)
        while (_context.Apartments.Any(a => a.InviteCode == inviteCode))
        {
            inviteCode = GenerateInviteCode();
        }

        // 3. Создаем сущность Квартиры
        var apartment = new Apartment
        {
            Name = request.Name,
            InviteCode = inviteCode
        };

        _context.Apartments.Add(apartment);

        // 4. Создаем связь: добавляем создателя как первого (и активного) жильца
        var member = new ApartmentMember
        {
            ApartmentId = apartment.Id,
            UserId = request.CreatorUserId,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        _context.ApartmentMembers.Add(member);

        // 5. Сохраняем всё в базу данных за одну транзакцию
        await _context.SaveChangesAsync(cancellationToken);

        // Возвращаем Успех и ID новой квартиры
        return Result<Guid>.Success(apartment.Id);
    }

    /// <summary>
    /// Вспомогательный метод для генерации 6-значного кода из заглавных букв и цифр
    /// </summary>
    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        
        // Берем 6 случайных символов из строки chars
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}