using System.Net;
using System.Net.Http.Json;
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
}
