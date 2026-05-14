using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Commands;

/// <summary>
/// Создаёт жалобу на сообщение. Идемпотентность через unique index
/// (MessageId, ReporterId) — повторный report от того же жильца обрабатывается
/// как успех без ошибки. Автор сообщения не может зарепортить сам себя.
/// Проверка доступа — по скоупу: для apartment-чата требует членства
/// в этой квартире, для building-чата — резидентства в этом здании.
/// </summary>
public record ReportMessageCommand(Guid MessageId, string ReporterId, string? Reason) : IRequest<Result<bool>>;

public class ReportMessageCommandHandler : IRequestHandler<ReportMessageCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public ReportMessageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(ReportMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return Result<bool>.Failure("Сообщение не найдено.");

        if (message.SenderId == request.ReporterId)
            return Result<bool>.Failure("Нельзя пожаловаться на собственное сообщение.");

        var hasAccess = message.Scope switch
        {
            ChatScope.Apartment => await IsActiveApartmentMember(message.ApartmentId!.Value, request.ReporterId, cancellationToken),
            ChatScope.Building => await IsActiveResidentOfBuilding(message.BuildingId!.Value, request.ReporterId, cancellationToken),
            _ => false
        };

        if (!hasAccess)
            return Result<bool>.Failure("Нет доступа к этому чату.");

        var alreadyReported = await _context.ChatMessageReports.AnyAsync(r =>
            r.MessageId == request.MessageId && r.ReporterId == request.ReporterId,
            cancellationToken);

        if (alreadyReported)
            return Result<bool>.Success(true); // idempotent

        _context.ChatMessageReports.Add(new ChatMessageReport
        {
            MessageId = request.MessageId,
            ReporterId = request.ReporterId,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim()
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    private Task<bool> IsActiveApartmentMember(Guid apartmentId, string userId, CancellationToken ct) =>
        _context.ApartmentMembers.AnyAsync(m =>
            m.ApartmentId == apartmentId && m.UserId == userId && m.IsActive, ct);

    private Task<bool> IsActiveResidentOfBuilding(Guid buildingId, string userId, CancellationToken ct) =>
        _context.ApartmentMembers
            .Where(m => m.UserId == userId && m.IsActive)
            .Join(_context.Apartments, m => m.ApartmentId, a => a.Id, (m, a) => a.BuildingId)
            .AnyAsync(b => b == buildingId, ct);
}
