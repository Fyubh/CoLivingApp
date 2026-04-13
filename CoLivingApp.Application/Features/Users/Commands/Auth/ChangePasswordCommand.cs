using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Users.Commands.Auth;

public record ChangePasswordCommand(string UserId, string OldPassword, string NewPassword) : IRequest<Result<Unit>>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public ChangePasswordCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return Result<Unit>.Failure("Пользователь не найден.");

        // Проверяем старый пароль
        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            return Result<Unit>.Failure("Неверный текущий пароль.");

        // Хешируем и сохраняем новый
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}