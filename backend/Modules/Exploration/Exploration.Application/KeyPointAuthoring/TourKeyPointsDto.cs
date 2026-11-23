using Exploration.Domain.Tours;

namespace Exploration.Application.KeyPointAuthoring;

public sealed record TourKeyPointsDto(
    string Name,
    TourStatus Status,
    List<KeyPointDto> KeyPoints,
    double? LengthKm);
