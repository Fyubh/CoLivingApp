// Файл: CoLivingApp.Application/Features/Apartments/Commands/JoinApartment/JoinApartmentCommandHandler.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Apartments.Commands.JoinApartment;

public class JoinApartmentCommandHandler : IRequestHandler<JoinApartmentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public JoinApartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(JoinApartmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Ищем квартиру по инвайт-коду (приводим к верхнему регистру для надежности)
        var apartment = await _context.Apartments
            .FirstOrDefaultAsync(a => a.InviteCode == request.InviteCode.ToUpper(), cancellationToken);

        if (apartment == null)
        {
            return Result<Guid>.Failure("Квартира с таким кодом не найдена.");
        }

        // 2. Проверяем, не является ли юзер уже жильцом этой квартиры
        var alreadyMember = await _context.ApartmentMembers
            .AnyAsync(m => m.ApartmentId == apartment.Id && m.UserId == request.UserId && m.IsActive, cancellationToken);

        if (alreadyMember)
        {
            return Result<Guid>.Failure("Вы уже являетесь участником этой квартиры.");
        }

        // 3. Создаем новую запись участника
        var member = new ApartmentMember
        {
            ApartmentId = apartment.Id,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ApartmentMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(apartment.Id);
    }
}