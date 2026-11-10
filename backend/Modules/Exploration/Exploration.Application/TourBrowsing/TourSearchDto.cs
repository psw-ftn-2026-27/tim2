using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourSearchDto(
    string? Name,
    TourDifficulty? Difficulty,
    List<string>? Tags,
    List<TransportMode>? TransportModes,
    DifficultySort DifficultySort = DifficultySort.None);
