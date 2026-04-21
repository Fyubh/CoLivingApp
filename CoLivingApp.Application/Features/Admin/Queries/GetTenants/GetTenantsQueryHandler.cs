using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Admin.Queries.GetTenants;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, Result<List<TenantDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TenantDto>>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        // 1. Берем из базы ТОЛЬКО жильцов (без персонала и админов)
        var query = _context.Users
            .Where(u => u.Role == UserRole.Tenant)
            .AsNoTracking();

        // 2. Поиск по имени или email, если админ ввел текст в строку поиска
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(searchLower) 
                                     || u.Email.ToLower().Contains(searchLower));
        }

        // 3. Сортировка по карме (по умолчанию - сначала с высокой кармой, но можно инвертировать)
        if (request.SortByKarmaAscending)
        {
            query = query.OrderBy(u => u.KarmaScore); // Сначала плохие парни
        }
        else
        {
            query = query.OrderByDescending(u => u.KarmaScore); // Сначала отличники
        }

        // 4. Маппинг (преобразование Entity в DTO)
        var tenants = await query
            .Select(u => new TenantDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                KarmaScore = u.KarmaScore
            })
            .ToListAsync(cancellationToken);

        return Result<List<TenantDto>>.Success(tenants);
    }
}