using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourLocationSearchRecording;
using Exploration.Domain.Locations;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourLocationSearchRecording;

public class TourLocationSearchRecordingCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/exploration/tour-location-searches";

    public TourLocationSearchRecordingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Confirmed_location_search_is_recorded_as_a_new_record_even_when_the_same_search_was_already_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = TourLocationSearchRecordSeed.NoviSadTenKmEasyWalkingHistoryWithTwoResults;
        var request = new LocationSearchRecordDto(seeded.Area.Center.Latitude, seeded.Area.Center.Longitude,
            seeded.Area.RadiusKm, seeded.Name, seeded.Difficulties, seeded.Tags, seeded.TransportModes,
            seeded.MinPrice, seeded.MaxPrice, seeded.MinLengthKm, seeded.MaxLengthKm, seeded.ResultCount);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var recordCountBefore = arrangeContext.TourLocationSearchRecords.Count();

        var response = await client.PostAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var recordId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        recordId.Should().NotBe(seeded.Id);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourLocationSearchRecords.Count().Should().Be(recordCountBefore + 1);
        var stored = assertContext.TourLocationSearchRecords.Single(record => record.Id == recordId);
        stored.Area.Center.Should().Be(new Location(seeded.Area.Center.Latitude, seeded.Area.Center.Longitude));
        stored.Area.RadiusKm.Should().Be(seeded.Area.RadiusKm);
        stored.Name.Should().Be(seeded.Name);
        stored.Difficulties.Should().Equal(TourDifficulty.Easy);
        stored.Tags.Should().Equal("istorija");
        stored.TransportModes.Should().Equal(TransportMode.Walking);
        stored.MinPrice.Should().Be(seeded.MinPrice);
        stored.MaxPrice.Should().Be(seeded.MaxPrice);
        stored.MinLengthKm.Should().Be(seeded.MinLengthKm);
        stored.MaxLengthKm.Should().Be(seeded.MaxLengthKm);
        stored.ResultCount.Should().Be(seeded.ResultCount);
    }

    [Fact]
    public async Task Location_search_with_a_radius_that_is_not_positive_is_rejected_and_nothing_is_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new LocationSearchRecordDto(45.2551, 19.8452, 0, null, null, null, null, null, null, null, null, 3);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var recordCountBefore = arrangeContext.TourLocationSearchRecords.Count();

        var response = await client.PostAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourLocationSearchRecords.Count().Should().Be(recordCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_record_a_location_search()
    {
        var client = Factory.CreateClient();
        var request = new LocationSearchRecordDto(45.2551, 19.8452, 10, null, null, null, null, null, null, null, null, 3);

        var response = await client.PostAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_record_a_location_search()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new LocationSearchRecordDto(45.2551, 19.8452, 10, null, null, null, null, null, null, null, null, 3);

        var response = await client.PostAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
