using Exploration.Application.TourPreferences;

namespace Exploration.Application.TourPreferenceSetting;

public sealed class TourPreferenceSettingQueries
{
    private readonly ITourPreferenceReadRepository _preferenceReadRepository;

    public TourPreferenceSettingQueries(ITourPreferenceReadRepository preferenceReadRepository)
    {
        _preferenceReadRepository = preferenceReadRepository;
    }

    public Task<TourPreferenceDto?> GetMineAsync(Guid touristId)
    {
        return _preferenceReadRepository.GetByTouristIdAsync(touristId);
    }
}
