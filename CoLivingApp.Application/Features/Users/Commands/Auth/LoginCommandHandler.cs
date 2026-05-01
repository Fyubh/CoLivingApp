using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoLivingApp.Application.Features.Users.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Email сравниваем case-insensitive — пользователь может ввести "Maria@..." или "maria@...".
        var emailLower = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<AuthTokenDto>.Failure("Неверный email или пароль.");

        var token = JwtIssuer.Issue(user, _configuration);
        return Result<AuthTokenDto>.Success(new AuthTokenDto(token, user.MustChangePassword));
    }
}

/// <summary>
/// Локальный helper для генерации JWT. Не вынесен в отдельный сервис,
/// потому что используется ровно в двух командах (Login и ChangePassword)
/// и зависит только от IConfiguration. Если появится третий потребитель
/// или Login переедет на refresh-token-flow — выносим в IJwtTokenService.
/// </summary>
internal static class JwtIssuer
{
    public static string Issue(User user, IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["JwtSettings:Secret"]!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("AccessLevel", user.AccessLevel.ToString())
        };

        // Claim добавляем ТОЛЬКО если флаг true — иначе токен после смены пароля
        // не должен содержать никаких следов "must change".
        if (user.MustChangePassword)
            claims.Add(new Claim("mustChangePassword", "true"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}