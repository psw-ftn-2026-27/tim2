using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourPreferences;

public sealed record PreferenceCriteria
{
    public List<TourDifficulty> Difficulties { get; private init; }
    public List<TransportMode> TransportModes { get; private init; }
    public List<string> Tags { get; private init; }

    private PreferenceCriteria()
    {
        Difficulties = null!;
        TransportModes = null!;
        Tags = null!;
    }

    public PreferenceCriteria(List<TourDifficulty> difficulties, List<TransportMode> transportModes, List<string> tags)
    {
        var nonEmptyTags = tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();

        var repeatedTag = nonEmptyTags
            .GroupBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (repeatedTag is not null)
        {
            throw new DomainException($"The tag \"{repeatedTag.Key}\" is listed more than once.");
        }
        if (difficulties.Count == 0 && transportModes.Count == 0 && nonEmptyTags.Count == 0)
        {
            throw new DomainException("Preferences require at least one difficulty, transport mode or tag.");
        }

        Difficulties = difficulties.Distinct().ToList();
        TransportModes = transportModes.Distinct().ToList();
        Tags = nonEmptyTags;
    }
}
