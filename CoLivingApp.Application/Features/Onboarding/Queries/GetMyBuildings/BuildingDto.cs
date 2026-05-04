namespace CoLivingApp.Application.Features.Onboarding.Queries.GetMyBuildings;

public record BuildingDto(
    Guid Id,
    string Name,
    string City,
    string Country,
    bool IsActive
);