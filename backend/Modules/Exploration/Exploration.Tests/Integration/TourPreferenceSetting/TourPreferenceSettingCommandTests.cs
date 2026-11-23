using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceSetting;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceSetting;

public class TourPreferenceSettingCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/exploration/tour-preferences/mine";

    public TourPreferenceSettingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_without_preferences_saves_them()
    {
        var touristId = Guid.NewGuid();
        var client = Factory.CreateClientFor(touristId, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Moderate], [TransportMode.Bicycle], ["vino"], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore + 1);
        var stored = assertContext.TourPreferences.Single(preference => preference.TouristId == touristId);
        stored.Criteria.Difficulties.Should().Equal(TourDifficulty.Moderate);
        stored.Criteria.TransportModes.Should().Equal(TransportMode.Bicycle);
        stored.Criteria.Tags.Should().Equal("vino");
        stored.ApplicableInSearch.Should().BeTrue();
    }

    [Fact]
    public async Task Tourist_with_preferences_saves_new_ones_which_replace_the_existing_ones_without_a_second_set()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Hard], [], ["planina", "jezero"], false);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        var stored = assertContext.TourPreferences.Single(preference => preference.TouristId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(TourPreferenceSeed.ExplorersEasyWalkingHistory.Id);
        stored.Criteria.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Criteria.TransportModes.Should().BeEmpty();
        stored.Criteria.Tags.Should().Equal("planina", "jezero");
        stored.ApplicableInSearch.Should().BeFalse();
    }

    [Fact]
    public async Task Tourist_cannot_save_empty_preferences_and_nothing_is_stored()
    {
        var touristId = Guid.NewGuid();
        var client = Factory.CreateClientFor(touristId, "explorer");
        var request = new TourPreferenceDto([], [], [], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        assertContext.TourPreferences.Should().NotContain(preference => preference.TouristId == touristId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_save_preferences()
    {
        var client = Factory.CreateClient();
        var request = new TourPreferenceDto([TourDifficulty.Easy], [], [], true);

        var response = await client.PutAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_save_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new TourPreferenceDto([TourDifficulty.Easy], [], [], true);

        var response = await client.PutAsJsonAsync(Url, request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tourist_deletes_their_preferences_together_with_the_option()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore - 1);
        assertContext.TourPreferences.Should().NotContain(preference => preference.TouristId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Tourist_without_preferences_cannot_delete_them()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_preferences()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
