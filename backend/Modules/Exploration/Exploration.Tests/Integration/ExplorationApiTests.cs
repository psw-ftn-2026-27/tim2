using Exploration.Contracts;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration;

public sealed class ExplorationApiTests : BaseIntegrationTest
{
    public ExplorationApiTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Published_tours_of_the_author_are_offered_with_their_identifier_name_author_and_price()
    {
        var expected = TourSeed.All
            .Where(tour => tour.AuthorId == WellKnownUsers.Explorer && tour.Status == TourStatus.Published)
            .Select(tour => new PublishedTourSummary(tour.Id, tour.Name, tour.AuthorId, tour.Price));
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var tours = await explorationApi.GetPublishedToursByAuthorAsync(WellKnownUsers.Explorer);

        tours.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Draft_and_archived_tours_of_the_author_are_not_offered()
    {
        var unpublished = TourSeed.All
            .Where(tour => tour.AuthorId == WellKnownUsers.Explorer && tour.Status != TourStatus.Published)
            .Select(tour => tour.Id);
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var tours = await explorationApi.GetPublishedToursByAuthorAsync(WellKnownUsers.Explorer);

        tours.Select(tour => tour.TourId).Should().NotIntersectWith(unpublished);
    }

    [Fact]
    public async Task Tours_of_another_author_are_not_offered()
    {
        var othersTours = TourSeed.All
            .Where(tour => tour.AuthorId != WellKnownUsers.Explorer)
            .Select(tour => tour.Id);
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var tours = await explorationApi.GetPublishedToursByAuthorAsync(WellKnownUsers.Explorer);

        tours.Select(tour => tour.TourId).Should().NotIntersectWith(othersTours);
    }

    [Fact]
    public async Task Author_without_published_tours_is_offered_no_tours()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var tours = await explorationApi.GetPublishedToursByAuthorAsync(WellKnownUsers.Administrator);

        tours.Should().BeEmpty();
    }
}
