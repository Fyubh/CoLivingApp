// Файл: CoLivingApp.Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Проверяем, нет ли уже такого пользователя (вдруг он просто залогинился заново)
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (existingUser != null)
        {
            // Если юзер уже есть, просто возвращаем Успех и его ID (ошибки здесь нет)
            return Result<string>.Success(existingUser.Id);
        }

        // 2. Создаем нового пользователя
        var user = new User
        {
            Id = request.Id,
            Email = request.Email,
            Name = request.Name
        };

        _context.Users.Add(user);
        
        // 3. Сохраняем в БД
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(user.Id);
    }
}