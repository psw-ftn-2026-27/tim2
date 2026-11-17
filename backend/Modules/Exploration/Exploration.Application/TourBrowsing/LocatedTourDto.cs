using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record LocatedTourDto(
    Guid Id,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    int Price,
    double MarkerLatitude,
    double MarkerLongitude);
