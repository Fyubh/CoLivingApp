using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Apartments.Queries.GetApartment;

public record GetApartmentQuery(Guid ApartmentId) : IRequest<Result<ApartmentDto>>;

public record ApartmentDto(Guid Id, string Name, string InviteCode, List<MemberDto> Members);
public record MemberDto(string UserId, string Name);

public class GetApartmentQueryHandler : IRequestHandler<GetApartmentQuery, Result<ApartmentDto>>
{
    private readonly IApplicationDbContext _context;
    public GetApartmentQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<ApartmentDto>> Handle(GetApartmentQuery request, CancellationToken cancellationToken)
    {
        var apartment = await _context.Apartments
            .Include(a => a.Members.Where(m => m.IsActive))
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(a => a.Id == request.ApartmentId, cancellationToken);

        if (apartment == null) return Result<ApartmentDto>.Failure("Квартира не найдена");

        var members = apartment.Members.Select(m => new MemberDto(m.UserId, m.User!.Name)).ToList();
        return Result<ApartmentDto>.Success(new ApartmentDto(apartment.Id, apartment.Name, apartment.InviteCode, members));
    }
}