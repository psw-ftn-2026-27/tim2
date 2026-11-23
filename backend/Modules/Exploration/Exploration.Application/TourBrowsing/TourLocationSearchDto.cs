using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourLocationSearchDto(
    double Latitude,
    double Longitude,
    double RadiusKm,
    string? Name,
    List<TourDifficulty>? Difficulties,
    List<string>? Tags,
    List<TransportMode>? TransportModes,
    int? MinPrice,
    int? MaxPrice,
    double? MinLengthKm,
    double? MaxLengthKm);
