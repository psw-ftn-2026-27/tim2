using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourLocationSearchRecords;

public sealed class TourLocationSearchRecord : AggregateRoot
{
    public SearchArea Area { get; private set; }
    public string? Name { get; private set; }
    public List<TourDifficulty> Difficulties { get; private set; }
    public List<string> Tags { get; private set; }
    public List<TransportMode> TransportModes { get; private set; }
    public int? MinPrice { get; private set; }
    public int? MaxPrice { get; private set; }
    public double? MinLengthKm { get; private set; }
    public double? MaxLengthKm { get; private set; }
    public int ResultCount { get; private set; }
    public DateTime SearchedAt { get; private set; }

    private TourLocationSearchRecord()
    {
        Area = null!;
        Difficulties = null!;
        Tags = null!;
        TransportModes = null!;
    }

    public TourLocationSearchRecord(SearchArea area, string? name, List<TourDifficulty>? difficulties,
        List<string>? tags, List<TransportMode>? transportModes, int? minPrice, int? maxPrice,
        double? minLengthKm, double? maxLengthKm, int resultCount)
        : base(Guid.NewGuid())
    {
        if (resultCount < 0)
        {
            throw new DomainException("A location search record cannot have a negative result count.");
        }

        Area = area;
        Name = name;
        Difficulties = difficulties ?? [];
        Tags = tags ?? [];
        TransportModes = transportModes ?? [];
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        MinLengthKm = minLengthKm;
        MaxLengthKm = maxLengthKm;
        ResultCount = resultCount;
        SearchedAt = DateTime.UtcNow;
    }
}
