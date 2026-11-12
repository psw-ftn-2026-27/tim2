using Exploration.Application.TourPreferenceSetting;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceReadRepository
{
    Task<TourPreferenceDto?> GetByTouristIdAsync(Guid touristId);
}
