namespace Exploration.Contracts;

public interface IExplorationApi
{
    Task<PublishedTourSummary?> GetPublishedTourAsync(Guid tourId);
}
