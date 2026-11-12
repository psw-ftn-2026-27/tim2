using Shared.Domain;

namespace Exploration.Domain.TourPreferences;

public sealed class TourPreference : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public PreferenceCriteria Criteria { get; private set; }
    public bool ApplicableInSearch { get; private set; }

    private TourPreference()
    {
        Criteria = null!;
    }

    public TourPreference(Guid touristId, PreferenceCriteria criteria, bool applicableInSearch)
        : base(Guid.NewGuid())
    {
        TouristId = touristId;
        Criteria = criteria;
        ApplicableInSearch = applicableInSearch;
    }

    public void Change(PreferenceCriteria criteria, bool applicableInSearch)
    {
        Criteria = criteria;
        ApplicableInSearch = applicableInSearch;
    }
}
