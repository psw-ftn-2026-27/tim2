using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    // A 3 km circle around Trg republike in Belgrade, where only the Belgrade tours of the seed have key points.
    private const string BelgradeCenterSearch = "/api/exploration/tours/published/by-location?latitude=44.8176&longitude=20.4569&radiusKm=3";

    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Only_published_tours_whose_title_contains_the_text_regardless_of_case_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?name=STAZA", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Tour_with_at_least_one_of_the_chosen_tags_is_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=vino&tags=jezero", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Tour_with_a_time_for_at_least_one_of_the_chosen_transport_modes_is_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?transportModes=Walking&transportModes=Car", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Tour_with_any_of_the_chosen_difficulties_is_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&difficulties=Hard&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Only_tours_meeting_every_given_filter_category_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>(
            "/api/exploration/tours/published?difficulties=Moderate&tags=priroda&tags=istorija&transportModes=Walking&transportModes=Bicycle", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Tours_sorted_by_ascending_difficulty_go_from_easy_through_moderate_to_hard()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficultySort=Ascending", JsonOptions);

        tours!.Items.Select(tour => tour.Difficulty).Should().Contain([TourDifficulty.Easy, TourDifficulty.Moderate, TourDifficulty.Hard]);
        tours.Items.Select(tour => tour.Difficulty).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Total_count_covers_every_matching_published_tour_when_only_one_page_is_returned()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=priroda&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published && tour.Tags.Contains("priroda")));
    }

    [Fact]
    public async Task Tour_priced_at_the_minimum_or_maximum_given_price_is_listed_and_a_tour_priced_outside_the_range_is_not()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?minPrice=500&maxPrice=1200", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedCheapGallery.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedModeratelyPricedFortress.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedFreeParkWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedExpensiveMountain.Id);
    }

    [Theory]
    [InlineData("minPrice=1000", 1000, int.MaxValue)]
    [InlineData("maxPrice=1000", 0, 1000)]
    public async Task Tours_are_narrowed_only_on_the_side_of_the_single_given_price_and_a_free_tour_in_range_is_listed(
        string priceBound, int lowestListedPrice, int highestListedPrice)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedIds = TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published && tour.Price >= lowestListedPrice && tour.Price <= highestListedPrice)
            .Select(tour => tour.Id);

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>($"/api/exploration/tours/published?{priceBound}&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task Only_tours_in_the_price_range_that_also_meet_another_filter_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&minPrice=500&maxPrice=3000", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedCheapGallery.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedModeratelyPricedFortress.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedExpensiveMountain.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedFreeParkWalk.Id);
    }

    [Fact]
    public async Task Tour_as_long_as_the_shortest_or_longest_given_length_is_listed_and_a_tour_outside_the_range_is_not()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?minLengthKm=1.2&maxLengthKm=4.2&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedModeratelyPricedFortress.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedCheapGallery.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Theory]
    [InlineData("minLengthKm=3", 3, double.MaxValue)]
    [InlineData("maxLengthKm=3", 0, 3)]
    public async Task Tours_are_narrowed_only_on_the_side_of_the_single_given_length(
        string lengthBound, double shortestListedLengthKm, double longestListedLengthKm)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedIds = TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published && tour.LengthKm >= shortestListedLengthKm && tour.LengthKm <= longestListedLengthKm)
            .Select(tour => tour.Id);

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>($"/api/exploration/tours/published?{lengthBound}&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task Only_tours_in_the_length_range_that_also_meet_another_filter_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&minLengthKm=1&maxLengthKm=5&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedFreeParkWalk.Id);
    }

    [Theory]
    [InlineData("Ascending", 1)]
    [InlineData("Descending", -1)]
    public async Task Tours_sorted_by_price_are_ordered_by_price_in_the_chosen_direction(string priceSort, int direction)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedPrices = TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published)
            .Select(tour => tour.Price)
            .OrderBy(price => direction * price);

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>($"/api/exploration/tours/published?priceSort={priceSort}&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => tour.Price).Should().Equal(expectedPrices);
    }

    [Fact]
    public async Task Tours_sorted_by_difficulty_and_price_are_grouped_by_difficulty_and_ordered_by_price_within_each_difficulty()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedOrder = TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published)
            .OrderBy(tour => tour.Difficulty)
            .ThenByDescending(tour => tour.Price)
            .Select(tour => (tour.Difficulty, tour.Price));

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficultySort=Ascending&priceSort=Descending&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => (tour.Difficulty, tour.Price)).Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task Archived_tour_is_not_listed_among_published_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Should().NotContain(tour => tour.Id == TourSeed.ArchivedCastle.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_browse_published_tours()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/exploration/tours/published");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Location_search_returns_exactly_the_published_tours_with_a_visible_key_point_in_the_area()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<LocatedTourDto>>($"{BelgradeCenterSearch}&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo(
            [TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Id]);
    }

    [Fact]
    public async Task Tour_whose_only_key_point_in_the_area_is_hidden_is_not_found_by_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<LocatedTourDto>>($"{BelgradeCenterSearch}&pageSize=100", JsonOptions);

        tours!.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Id);
    }

    [Fact]
    public async Task Tour_is_marked_at_its_first_visible_key_point_even_when_that_point_is_outside_the_area()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var firstVisibleLocation = TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.KeyPoints[1].Location;

        var tours = await client.GetFromJsonAsync<PageResult<LocatedTourDto>>($"{BelgradeCenterSearch}&pageSize=100", JsonOptions);

        var markedTour = tours!.Items.Single(tour => tour.Id == TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Id);
        markedTour.MarkerLatitude.Should().Be(firstVisibleLocation.Latitude);
        markedTour.MarkerLongitude.Should().Be(firstVisibleLocation.Longitude);
    }

    [Fact]
    public async Task Location_search_with_a_price_limit_returns_only_tours_in_the_area_that_also_meet_the_price()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<LocatedTourDto>>($"{BelgradeCenterSearch}&maxPrice=1000&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo([TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id]);
    }

    [Theory]
    [InlineData("minLengthKm=10", 10, double.MaxValue)]
    [InlineData("maxLengthKm=10", 0, 10)]
    public async Task Location_search_with_a_length_limit_returns_only_tours_in_the_area_in_that_range_each_with_its_length(
        string lengthBound, double shortestListedLengthKm, double longestListedLengthKm)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        Tour[] toursInArea = [TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade];
        var expectedTours = toursInArea
            .Where(tour => tour.LengthKm >= shortestListedLengthKm && tour.LengthKm <= longestListedLengthKm)
            .Select(tour => (tour.Id, tour.LengthKm));

        var tours = await client.GetFromJsonAsync<PageResult<LocatedTourDto>>($"{BelgradeCenterSearch}&{lengthBound}&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => (tour.Id, tour.LengthKm)).Should().BeEquivalentTo(expectedTours);
    }

    [Fact]
    public async Task Total_count_covers_every_tour_in_the_area_when_only_one_page_is_returned()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        Guid[] toursInArea = [TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Id];

        var tours = await client.GetFromJsonAsync<PageResult<LocatedTourDto>>($"{BelgradeCenterSearch}&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(toursInArea.Length);
    }

    [Fact]
    public async Task Anonymous_user_cannot_search_tours_by_location()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(BelgradeCenterSearch);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Guest_gets_only_the_three_most_recently_first_published_tours_newest_first_even_when_asking_for_a_larger_page()
    {
        var client = Factory.CreateClient();
        var published = TourSeed.All.Where(tour => tour.Status == TourStatus.Published).ToList();

        var tours = await client.GetFromJsonAsync<List<FeaturedTourDto>>("/api/exploration/tours/featured?page=1&pageSize=100", JsonOptions);

        var featuredNames = tours!.Select(tour => tour.Name).ToList();
        var featuredTimes = featuredNames.Select(name => PublishedMicroseconds(published.Single(tour => tour.Name == name))).ToList();
        var newerTourLeftOut = published
            .Where(tour => !featuredNames.Contains(tour.Name))
            .Any(tour => PublishedMicroseconds(tour) > featuredTimes.Min());
        featuredTimes.Should().HaveCount(Math.Min(3, published.Count)).And.BeInDescendingOrder();
        newerTourLeftOut.Should().BeFalse();
    }

    // The database keeps publishing times to the microsecond, so tours published within one microsecond tie there.
    private static long PublishedMicroseconds(Tour tour)
    {
        return tour.PublishedAt!.Value.Ticks / 10;
    }

    [Fact]
    public async Task Draft_and_archived_tours_are_not_featured()
    {
        var client = Factory.CreateClient();

        var tours = await client.GetFromJsonAsync<List<FeaturedTourDto>>("/api/exploration/tours/featured", JsonOptions);

        tours!.Select(tour => tour.Name).Should().NotContain([TourSeed.FortressWalk.Name, TourSeed.PublishableRiverside.Name, TourSeed.ArchivedCastle.Name]);
    }

    [Fact]
    public async Task Guest_gets_only_the_name_description_and_numeric_price_of_each_featured_tour()
    {
        var client = Factory.CreateClient();

        var body = await client.GetStringAsync("/api/exploration/tours/featured");

        using var json = JsonDocument.Parse(body);
        json.RootElement.EnumerateArray().Should().NotBeEmpty().And.AllSatisfy(tour =>
        {
            tour.EnumerateObject().Select(field => field.Name).Should().BeEquivalentTo(["name", "description", "price"]);
            tour.GetProperty("price").ValueKind.Should().Be(JsonValueKind.Number);
        });
    }

    [Theory]
    [MemberData(nameof(PublishedToursWithHiddenKeyPoints))]
    public async Task Buyer_who_has_not_purchased_the_tour_sees_only_the_key_points_visible_before_purchase_in_order(Guid tourId)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedNames = TourSeed.All
            .Single(tour => tour.Id == tourId)
            .KeyPoints
            .Where(keyPoint => keyPoint.IsVisibleBeforePurchase)
            .OrderBy(keyPoint => keyPoint.Order)
            .Select(keyPoint => keyPoint.Name);

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tourId}", JsonOptions);

        details!.Id.Should().Be(tourId);
        details.IsPurchased.Should().BeFalse();
        details.KeyPoints.Select(keyPoint => keyPoint.Name).Should().Equal(expectedNames);
    }

    public static TheoryData<Guid> PublishedToursWithHiddenKeyPoints => new()
    {
        TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id,
        TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Id
    };

    [Fact]
    public async Task Tour_details_never_carry_the_secret_of_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedSynagogueWithSecretOnVisibleKeyPoint;
        var visibleSecrets = tour.KeyPoints
            .Where(keyPoint => keyPoint.IsVisibleBeforePurchase && keyPoint.Secret != null)
            .Select(keyPoint => keyPoint.Secret!)
            .ToList();

        var body = await client.GetStringAsync($"/api/exploration/tours/{tour.Id}");

        visibleSecrets.Should().NotBeEmpty();
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("keyPoints").EnumerateArray().Should().NotBeEmpty().And.AllSatisfy(keyPoint =>
            keyPoint.EnumerateObject().Select(field => field.Name).Should().NotContain("secret"));
        visibleSecrets.Should().AllSatisfy(secret => body.Should().NotContain(secret));
    }

    [Fact]
    public async Task Archived_tour_is_not_available_to_a_buyer_who_has_not_purchased_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.ArchivedTasmajdanWithVisibleKeyPointInBelgrade.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_open_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
