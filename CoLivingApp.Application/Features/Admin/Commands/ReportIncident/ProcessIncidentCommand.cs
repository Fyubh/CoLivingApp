using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Admin.Commands.ProcessIncident;

public class ProcessIncidentCommand : IRequest<Result<bool>>
{
    public string IncidentId { get; set; } = string.Empty;
    public bool IsApproved { get; set; } // true - штрафуем, false - прощаем
}

public class ProcessIncidentCommandHandler : IRequestHandler<ProcessIncidentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public ProcessIncidentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(ProcessIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken);

        if (incident == null) return Result<bool>.Failure("Инцидент не найден.");

        if (request.IsApproved)
        {
            incident.Status = IncidentStatus.Approved;
            
            // Наказываем студента: минус 15 Кармы за грязь
            if (incident.User != null)
            {
                incident.User.KarmaScore -= 15;
                // Для MVP: можно не создавать пока реальный Expense, достаточно падения кармы
            }
        }
        else
        {
            incident.Status = IncidentStatus.Rejected;
            // ИИ ошибся, карму не трогаем
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}