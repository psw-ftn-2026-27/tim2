namespace Exploration.Contracts;

public interface IExplorationApi
{
    Task<PublishedTourSummary?> GetPublishedTourAsync(Guid tourId);
    Task<List<PublishedTourSummary>> GetPublishedToursByAuthorAsync(Guid authorId);
}
