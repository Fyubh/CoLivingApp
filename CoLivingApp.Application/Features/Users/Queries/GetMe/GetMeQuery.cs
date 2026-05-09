using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Users.Queries.GetMe;

/// <summary>
/// Текущий пользователь — для экрана «Профиль» и для возможной валидации
/// сессии при холодном старте (Phase 8). UserId берём из JWT-клейма
/// в контроллере, в команду передаём явно — Application слой не знает
/// про HTTP-контекст.
/// </summary>
public record GetMeQuery(string UserId) : IRequest<Result<MeDto>>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<MeDto>>
{
    private readonly IApplicationDbContext _context;
    public GetMeQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MeDto>> Handle(GetMeQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result<MeDto>.Failure("Не указан пользователь.");

        var dto = await _context.Users
            .Where(u => u.Id == request.UserId)
            .Select(u => new MeDto(
                u.Id,
                u.Email,
                u.Name,
                u.Role.ToString(),
                u.AccessLevel))
            .FirstOrDefaultAsync(ct);

        return dto == null
            ? Result<MeDto>.Failure("Пользователь не найден.")
            : Result<MeDto>.Success(dto);
    }
}
