using Exploration.Domain.Tours;

namespace Exploration.Application.TourPreferenceSetting;

public sealed record TourPreferenceDto(
    List<TourDifficulty> Difficulties,
    List<TransportMode> TransportModes,
    List<string> Tags,
    bool ApplicableInSearch);
