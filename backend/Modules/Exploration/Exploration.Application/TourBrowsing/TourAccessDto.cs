using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

// What the tour details query needs to decide who may open the tour before it reads the details.
public sealed record TourAccessDto(Guid AuthorId, TourStatus Status);
