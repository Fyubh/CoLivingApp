using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Users.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RegisterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return Result<string>.Failure("Пользователь с таким Email уже существует.");

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            // Хешируем пароль с помощью BCrypt
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password) 
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(user.Id);
    }
}