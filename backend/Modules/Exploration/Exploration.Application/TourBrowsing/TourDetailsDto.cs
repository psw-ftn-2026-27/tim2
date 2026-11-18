using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourDetailsDto(
    Guid Id,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    int Price,
    double? LengthKm,
    RecommendedGroupSizeDto? RecommendedGroupSize,
    List<TransportTimeDto> TransportTimes,
    List<TourDetailsKeyPointDto> KeyPoints,
    bool IsPurchased);
