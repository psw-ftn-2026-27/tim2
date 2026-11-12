using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceSetting;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceSetting;

public class TourPreferenceSettingQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/exploration/tour-preferences/mine";

    public TourPreferenceSettingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_gets_their_own_preferences_and_not_someone_elses()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var preference = await client.GetFromJsonAsync<TourPreferenceDto>(Url, JsonOptions);

        var seeded = TourPreferenceSeed.ExplorersEasyWalkingHistory;
        preference!.Difficulties.Should().Equal(seeded.Criteria.Difficulties);
        preference.TransportModes.Should().Equal(seeded.Criteria.TransportModes);
        preference.Tags.Should().Equal(seeded.Criteria.Tags);
        preference.ApplicableInSearch.Should().Be(seeded.ApplicableInSearch);
    }

    [Fact]
    public async Task Tourist_without_preferences_gets_an_empty_response()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.GetAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }
}
