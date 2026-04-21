using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Admin.Commands.ReportIncident;

public class ReportAiIncidentCommand : IRequest<Result<string>>
{
    public string UserId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AiConfidenceScore { get; set; }
}

public class ReportAiIncidentCommandHandler : IRequestHandler<ReportAiIncidentCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public ReportAiIncidentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(ReportAiIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = new Incident
        {
            UserId = request.UserId,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            AiConfidenceScore = request.AiConfidenceScore
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(cancellationToken);

        // В реальном приложении здесь можно кинуть SignalR уведомление Админу на дашборд
        return Result<string>.Success(incident.Id);
    }
}