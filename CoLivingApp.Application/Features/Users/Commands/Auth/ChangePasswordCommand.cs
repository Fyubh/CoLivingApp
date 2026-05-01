using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CoLivingApp.Application.Features.Users.Commands.Auth;

/// <summary>
/// Смена пароля. Используется и для регулярной смены, и для первого входа после
/// получения временного пароля от админа.
///
/// На входе нужен старый пароль — даже на первом входе. Юзер только что вводил
/// временный пароль на форме логина, помнит его. Безопасность важнее микро-удобства:
/// если бы старый пароль не требовался, любой, кто перехватил JWT, мог бы поменять
/// пароль.
///
/// На выходе — НОВЫЙ JWT (с обновлёнными claim'ами, без mustChangePassword).
/// Старый JWT всё ещё технически валиден до своей exp-даты — фронт обязан
/// заменить его в localStorage немедленно.
/// </summary>
public record ChangePasswordCommand(string UserId, string OldPassword, string NewPassword)
    : IRequest<Result<AuthTokenDto>>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<AuthTokenDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public ChangePasswordCommandHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<AuthTokenDto>> Handle(
        ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return Result<AuthTokenDto>.Failure("Новый пароль должен быть не короче 8 символов.");

        if (request.OldPassword == request.NewPassword)
            return Result<AuthTokenDto>.Failure("Новый пароль должен отличаться от старого.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            return Result<AuthTokenDto>.Failure("Пользователь не найден.");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            return Result<AuthTokenDto>.Failure("Неверный текущий пароль.");

        // Сохраняем новый хэш и сбрасываем флаг.
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.MustChangePassword = false;

        await _context.SaveChangesAsync(cancellationToken);

        // Выдаём новый JWT — без claim'а mustChangePassword, фронт сразу
        // его сохраняет и продолжает работу.
        var token = JwtIssuer.Issue(user, _configuration);
        return Result<AuthTokenDto>.Success(new AuthTokenDto(token, MustChangePassword: false));
    }
}