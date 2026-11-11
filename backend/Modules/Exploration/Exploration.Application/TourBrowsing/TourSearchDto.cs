using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourSearchDto(
    string? Name,
    TourDifficulty? Difficulty,
    List<string>? Tags,
    List<TransportMode>? TransportModes,
    int? MinPrice,
    int? MaxPrice,
    SortDirection DifficultySort = SortDirection.None,
    SortDirection PriceSort = SortDirection.None);
