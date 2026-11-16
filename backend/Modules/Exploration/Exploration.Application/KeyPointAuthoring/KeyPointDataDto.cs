namespace Exploration.Application.KeyPointAuthoring;

public sealed record KeyPointDataDto(
    string Name,
    string Description,
    string ImageUrl,
    double Latitude,
    double Longitude,
    string? Secret,
    bool IsVisibleBeforePurchase);
