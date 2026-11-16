namespace Exploration.Contracts;

public sealed record PublishedTourSummary(Guid TourId, string Name, Guid AuthorId, int Price);
