using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Admin.Queries.GetDashboard;

public record DashboardStatsDto(
    int TotalTenants,
    double AverageAiCleanlinessScore, // Средняя карма по зданию
    int PendingIncidentsCount,
    List<string> CriticalAlerts
);

public class GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>> { }

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context;
    public GetDashboardStatsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // 1. Всего жильцов
        var tenantsCount = await _context.Users.CountAsync(u => u.Role == UserRole.Tenant, cancellationToken);

        // 2. Индекс чистоты (средняя карма по зданию)
        var avgKarma = await _context.Users
            .Where(u => u.Role == UserRole.Tenant)
            .AverageAsync(u => (double?)u.KarmaScore, cancellationToken) ?? 100.0;

        // 3. Сколько инцидентов ждут решения админа
        var pendingIncidents = await _context.Incidents
            .CountAsync(i => i.Status == IncidentStatus.PendingAI, cancellationToken);

        // 4. Фейковые/Динамические алерты для красивого UI
        var alerts = new List<string>();
        if (pendingIncidents > 5) alerts.Add("⚠️ Критический уровень необработанных инцидентов от ИИ!");
        if (avgKarma < 70) alerts.Add("🚨 Общий уровень дисциплины здания упал ниже нормы.");
        alerts.Add("🔔 Заканчивается туалетная бумага на 4 этаже (Блок B)."); // Фейк для демо Procurement модуля

        var stats = new DashboardStatsDto(
            tenantsCount, 
            Math.Round(avgKarma, 1), 
            pendingIncidents, 
            alerts
        );

        return Result<DashboardStatsDto>.Success(stats);
    }
}