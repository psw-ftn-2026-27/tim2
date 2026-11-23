using Exploration.Application.TourPreferences;
using Exploration.Domain.TourPreferences;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPreferenceSetting;

public sealed class TourPreferenceSettingService
{
    private readonly ITourPreferenceRepository _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourPreferenceSettingService(ITourPreferenceRepository preferenceRepository, IUnitOfWork unitOfWork)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SaveAsync(Guid touristId, TourPreferenceDto dto)
    {
        var criteria = new PreferenceCriteria(dto.Difficulties, dto.TransportModes, dto.Tags);
        var preference = await _preferenceRepository.GetByTouristIdAsync(touristId);

        if (preference is null)
        {
            _preferenceRepository.Add(new TourPreference(touristId, criteria, dto.ApplicableInSearch));
        }
        else
        {
            preference.Change(criteria, dto.ApplicableInSearch);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid touristId)
    {
        var preference = await _preferenceRepository.GetByTouristIdAsync(touristId);
        if (preference is null)
        {
            throw new NotFoundException("The tourist has no tour preferences.");
        }

        _preferenceRepository.Remove(preference);
        await _unitOfWork.SaveChangesAsync();
    }
}
