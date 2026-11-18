namespace Exploration.Application.TourBrowsing;

// Carries no secret: the buyer unlocks it only at the key point itself.
public sealed record TourDetailsKeyPointDto(
    int Order,
    string Name,
    string Description,
    string ImageUrl,
    double Latitude,
    double Longitude);
