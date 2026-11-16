namespace Exploration.Application.KeyPointAuthoring;

public sealed record KeyPointDto(
    Guid Id,
    int Order,
    string Name,
    string Description,
    string ImageUrl,
    double Latitude,
    double Longitude,
    string? Secret,
    bool IsVisibleBeforePurchase);
