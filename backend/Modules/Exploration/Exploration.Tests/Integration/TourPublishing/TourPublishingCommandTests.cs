using System.Net;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPublishing;

public class TourPublishingCommandTests : BaseIntegrationTest
{
    public TourPublishingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tour_is_published_when_all_rules_are_met()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Author_archives_their_own_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var publishedAtBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id).PublishedAt;

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Archived);
        stored.ArchivedAt.Should().NotBeNull();
        stored.PublishedAt.Should().Be(publishedAtBefore);
    }

    [Fact]
    public async Task Archiving_a_draft_tour_is_rejected_and_the_tour_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Explorer_cannot_archive_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedLakeside.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_archive_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_reactivates_their_own_archived_tour_and_it_keeps_the_original_publishing_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var publishedAtBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedCastle.Id).PublishedAt;

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedCastle.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedCastle.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
        stored.PublishedAt.Should().Be(publishedAtBefore);
    }

    [Fact]
    public async Task Reactivating_a_published_tour_is_rejected_and_the_tour_stays_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var publishedAtBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id).PublishedAt;

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
        stored.PublishedAt.Should().Be(publishedAtBefore);
    }

    [Fact]
    public async Task Explorer_cannot_reactivate_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var archivedAtBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedCastle.Id).ArchivedAt;

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedCastle.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedCastle.Id);
        stored.Status.Should().Be(TourStatus.Archived);
        stored.ArchivedAt.Should().Be(archivedAtBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reactivate_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedCastle.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reactivate_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedCastle.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
