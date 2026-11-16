using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);

    private static Tour CreateTour(string description)
    {
        var tour = new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"]);
        tour.ChangeRecommendedGroupSize(2, 8);
        return tour;
    }

    private static Tour CreatePublishedTour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.Publish();
        return tour;
    }

    private static Tour CreateTourWithKeyPoints(params string[] names)
    {
        var tour = CreateTour(LongDescription);
        foreach (var name in names)
        {
            tour.AddKeyPoint(name, "Opis tačke.", "/images/tacka.jpg", new Location(45.25, 19.85), null, false);
        }
        return tour;
    }

    private static Tour CreatePublishedTourWithKeyPoint()
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća");
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.Publish();
        return tour;
    }

    private static int OrderOf(Tour tour, string name)
    {
        return tour.KeyPoints.Single(keyPoint => keyPoint.Name == name).Order;
    }

    [Fact]
    public void New_tour_starts_as_a_draft()
    {
        var tour = CreateTour(LongDescription);

        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
        tour.TransportTimes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_name(string name)
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_description(string description)
    {
        var creation = () => CreateTour(description);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_has_one_time_per_transport_mode()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, 90);

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Transport_time_must_be_positive(int minutes)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, minutes);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_is_published_when_all_rules_are_met()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Bicycle, 45);

        tour.Publish();

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Tour_with_a_short_description_cannot_be_published()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_recommended_group_size_cannot_be_published()
    {
        var tour = new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", LongDescription, TourDifficulty.Easy, ["istorija"]);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    [InlineData(4, 3)]
    public void Recommended_group_size_requires_a_minimum_of_at_least_one_and_a_maximum_not_below_it(int min, int max)
    {
        var creation = () => new RecommendedGroupSize(min, max);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_change_its_recommended_group_size()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.Publish();

        var change = () => tour.ChangeRecommendedGroupSize(3, 6);

        change.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-500)]
    public void Tour_price_cannot_be_negative(int price)
    {
        var tour = CreateTour(LongDescription);

        var change = () => tour.ChangePrice(price);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_that_is_not_a_draft_cannot_change_its_price()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.Publish();

        var change = () => tour.ChangePrice(800);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Car, 30);
        tour.Publish();

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_published()
    {
        var tour = CreatePublishedTour();
        tour.Archive();

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_that_is_not_published_cannot_be_archived()
    {
        var tour = CreateTour(LongDescription);

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archiving_a_published_tour_records_the_archiving_time()
    {
        var tour = CreatePublishedTour();

        tour.Archive();

        tour.Status.Should().Be(TourStatus.Archived);
        tour.ArchivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Tour_that_is_not_archived_cannot_be_reactivated()
    {
        var tour = CreatePublishedTour();

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reactivation_returns_the_tour_to_published_clears_the_archiving_time_and_keeps_the_original_publishing_time()
    {
        var tour = CreatePublishedTour();
        var publishedAt = tour.PublishedAt;
        tour.Archive();

        tour.Reactivate();

        tour.Status.Should().Be(TourStatus.Published);
        tour.ArchivedAt.Should().BeNull();
        tour.PublishedAt.Should().Be(publishedAt);
    }

    [Theory]
    [InlineData(90.5, 0)]
    [InlineData(-90.5, 0)]
    [InlineData(0, 180.5)]
    [InlineData(0, -180.5)]
    public void Location_requires_a_latitude_from_minus_90_to_90_and_a_longitude_from_minus_180_to_180(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Opis tačke.", "/images/tacka.jpg")]
    [InlineData("Gradska kuća", "   ", "/images/tacka.jpg")]
    [InlineData("Gradska kuća", "Opis tačke.", "")]
    public void Key_point_without_a_name_description_or_image_is_not_added(string name, string description, string imageUrl)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddKeyPoint(name, description, imageUrl, new Location(45.25, 19.85), null, false);

        addition.Should().Throw<DomainException>();
        tour.KeyPoints.Should().BeEmpty();
    }

    [Fact]
    public void Key_point_without_a_secret_is_added()
    {
        var tour = CreateTour(LongDescription);

        tour.AddKeyPoint("Gradska kuća", "Opis tačke.", "/images/tacka.jpg", new Location(45.25, 19.85), null, true);

        tour.KeyPoints.Should().ContainSingle(keyPoint => keyPoint.Name == "Gradska kuća" && keyPoint.Secret == null);
    }

    [Theory]
    [InlineData("   ", "Opis tačke.", "/images/tacka.jpg")]
    [InlineData("Gradska kuća", "", "/images/tacka.jpg")]
    [InlineData("Gradska kuća", "Opis tačke.", "   ")]
    public void Key_point_cannot_be_changed_to_have_no_name_description_or_image(string name, string description, string imageUrl)
    {
        var tour = CreateTourWithKeyPoints("Županija");
        var keyPointId = tour.KeyPoints[0].Id;

        var change = () => tour.ChangeKeyPoint(keyPointId, name, description, imageUrl, new Location(45.25, 19.85), null, false);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_key_point_gets_the_next_order_at_the_end_of_the_list()
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća", "Županija");

        tour.AddKeyPoint("Samostan", "Opis tačke.", "/images/tacka.jpg", new Location(45.25, 19.85), null, false);

        OrderOf(tour, "Samostan").Should().Be(3);
    }

    [Fact]
    public void Key_points_after_a_removed_one_move_one_place_forward_without_a_gap()
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća", "Županija", "Samostan", "Pozorište");
        var removedId = tour.KeyPoints.Single(keyPoint => keyPoint.Name == "Županija").Id;

        tour.RemoveKeyPoint(removedId);

        tour.KeyPoints.Select(keyPoint => keyPoint.Name).Should().BeEquivalentTo(["Gradska kuća", "Samostan", "Pozorište"]);
        OrderOf(tour, "Gradska kuća").Should().Be(1);
        OrderOf(tour, "Samostan").Should().Be(2);
        OrderOf(tour, "Pozorište").Should().Be(3);
    }

    [Theory]
    [InlineData(KeyPointMoveDirection.Up, "Gradska kuća", 1)]
    [InlineData(KeyPointMoveDirection.Down, "Samostan", 3)]
    public void Moving_a_key_point_swaps_its_place_with_the_neighbouring_one(KeyPointMoveDirection direction, string neighbour, int newOrder)
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća", "Županija", "Samostan");
        var movedId = tour.KeyPoints.Single(keyPoint => keyPoint.Name == "Županija").Id;

        tour.MoveKeyPoint(movedId, direction);

        OrderOf(tour, "Županija").Should().Be(newOrder);
        OrderOf(tour, neighbour).Should().Be(2);
    }

    [Theory]
    [InlineData("Gradska kuća", KeyPointMoveDirection.Up)]
    [InlineData("Samostan", KeyPointMoveDirection.Down)]
    public void First_key_point_cannot_move_up_and_last_cannot_move_down(string name, KeyPointMoveDirection direction)
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća", "Županija", "Samostan");
        var movedId = tour.KeyPoints.Single(keyPoint => keyPoint.Name == name).Id;

        var move = () => tour.MoveKeyPoint(movedId, direction);

        move.Should().Throw<DomainException>();
    }

    [Fact]
    public void Missing_key_point_cannot_be_changed()
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća");

        var change = () => tour.ChangeKeyPoint(Guid.NewGuid(), "Županija", "Opis tačke.", "/images/tacka.jpg", new Location(45.25, 19.85), null, false);

        change.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Missing_key_point_cannot_be_removed()
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća");

        var removal = () => tour.RemoveKeyPoint(Guid.NewGuid());

        removal.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Missing_key_point_cannot_be_moved()
    {
        var tour = CreateTourWithKeyPoints("Gradska kuća", "Županija");

        var move = () => tour.MoveKeyPoint(Guid.NewGuid(), KeyPointMoveDirection.Down);

        move.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Tour_that_is_not_a_draft_cannot_get_a_new_key_point()
    {
        var tour = CreatePublishedTour();

        var addition = () => tour.AddKeyPoint("Gradska kuća", "Opis tačke.", "/images/tacka.jpg", new Location(45.25, 19.85), null, false);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_that_is_not_a_draft_cannot_change_a_key_point()
    {
        var tour = CreatePublishedTourWithKeyPoint();
        var keyPointId = tour.KeyPoints[0].Id;

        var change = () => tour.ChangeKeyPoint(keyPointId, "Županija", "Opis tačke.", "/images/tacka.jpg", new Location(45.25, 19.85), null, false);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_that_is_not_a_draft_cannot_remove_a_key_point()
    {
        var tour = CreatePublishedTourWithKeyPoint();
        var keyPointId = tour.KeyPoints[0].Id;

        var removal = () => tour.RemoveKeyPoint(keyPointId);

        removal.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_that_is_not_a_draft_cannot_move_a_key_point()
    {
        var tour = CreatePublishedTourWithKeyPoint();
        var keyPointId = tour.KeyPoints[0].Id;

        var move = () => tour.MoveKeyPoint(keyPointId, KeyPointMoveDirection.Down);

        move.Should().Throw<DomainException>();
    }
}
