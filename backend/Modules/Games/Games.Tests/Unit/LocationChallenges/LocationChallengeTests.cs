using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeTests
{
    private const string ValidName = "Skriveni kutci tvrđave";
    private const int ValidXp = 100;
    private const double StartLatitude = 45.2550;
    private const double StartLongitude = 19.8600;

    // One thousandth of a degree of latitude is about 111 m, so these offsets are easy to read as distances.
    private static Location LocationNorthOfStart(double latitudeOffset) => new(StartLatitude + latitudeOffset, StartLongitude);

    // Points are about 56 m apart along a line to the north, so even eleven of them stay within 1000 m of the start.
    private static List<HiddenPoint> CreateHiddenPoints(int count)
    {
        var hiddenPoints = new List<HiddenPoint>();
        for (var number = 1; number <= count; number++)
        {
            hiddenPoints.Add(new HiddenPoint($"/uploads/tacka-{number}.jpg", LocationNorthOfStart(0.0005 * number)));
        }
        return hiddenPoints;
    }

    private static LocationChallenge CreateChallenge() => new(ValidName, ValidXp, new Location(StartLatitude, StartLongitude), CreateHiddenPoints(2));

    private static LocationChallenge CreateChallengeIn(LocationChallengeStatus status)
    {
        var challenge = CreateChallenge();
        if (status == LocationChallengeStatus.Draft)
        {
            return challenge;
        }
        challenge.Publish();
        if (status == LocationChallengeStatus.Published)
        {
            return challenge;
        }
        challenge.Retire();
        if (status == LocationChallengeStatus.Retired)
        {
            return challenge;
        }
        challenge.Archive();
        return challenge;
    }

    private static void Perform(LocationChallenge challenge, string action)
    {
        switch (action)
        {
            case "publish":
                challenge.Publish();
                break;
            case "retire":
                challenge.Retire();
                break;
            case "archive":
                challenge.Archive();
                break;
            case "repeat":
                challenge.Repeat();
                break;
            default:
                throw new ArgumentException($"Unknown action {action}.", nameof(action));
        }
    }

    [Fact]
    public void New_location_challenge_starts_as_a_draft_with_the_given_name_xp_start_and_hidden_points()
    {
        var startLocation = new Location(StartLatitude, StartLongitude);
        var hiddenPoints = CreateHiddenPoints(3);

        var challenge = new LocationChallenge("  Skriveni kutci tvrđave  ", ValidXp, startLocation, hiddenPoints);

        challenge.Status.Should().Be(LocationChallengeStatus.Draft);
        challenge.Name.Should().Be(ValidName);
        challenge.Xp.Should().Be(ValidXp);
        challenge.StartLocation.Should().Be(startLocation);
        challenge.HiddenPoints.Should().Equal(hiddenPoints);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901")]
    public void Location_challenge_with_a_name_outside_the_allowed_length_cannot_be_created(string name)
    {
        var creation = () => new LocationChallenge(name, ValidXp, new Location(StartLatitude, StartLongitude), CreateHiddenPoints(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Location_challenge_with_xp_outside_the_allowed_range_cannot_be_created(int xp)
    {
        var creation = () => new LocationChallenge(ValidName, xp, new Location(StartLatitude, StartLongitude), CreateHiddenPoints(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Location_challenge_without_hidden_points_or_with_more_than_ten_cannot_be_created(int count)
    {
        var creation = () => new LocationChallenge(ValidName, ValidXp, new Location(StartLatitude, StartLongitude), CreateHiddenPoints(count));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_challenge_with_a_hidden_point_farther_than_one_thousand_meters_from_the_start_cannot_be_created()
    {
        var hiddenPoints = new List<HiddenPoint> { new("/uploads/daleko.jpg", LocationNorthOfStart(0.0091)) };

        var creation = () => new LocationChallenge(ValidName, ValidXp, new Location(StartLatitude, StartLongitude), hiddenPoints);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_challenge_with_a_hidden_point_closer_than_twenty_meters_to_the_start_cannot_be_created()
    {
        var hiddenPoints = new List<HiddenPoint> { new("/uploads/blizu.jpg", LocationNorthOfStart(0.00017)) };

        var creation = () => new LocationChallenge(ValidName, ValidXp, new Location(StartLatitude, StartLongitude), hiddenPoints);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_challenge_with_two_hidden_points_closer_than_twenty_meters_to_each_other_cannot_be_created()
    {
        var hiddenPoints = new List<HiddenPoint>
        {
            new("/uploads/prva.jpg", LocationNorthOfStart(0.0010)),
            new("/uploads/druga.jpg", LocationNorthOfStart(0.00117))
        };

        var creation = () => new LocationChallenge(ValidName, ValidXp, new Location(StartLatitude, StartLongitude), hiddenPoints);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Changing_a_draft_replaces_its_name_xp_start_and_all_hidden_points()
    {
        var challenge = CreateChallenge();
        var newStartLocation = new Location(45.2551, 19.8451);
        var newHiddenPoints = new List<HiddenPoint> { new("/uploads/nova.jpg", new Location(45.2565, 19.8451)) };

        challenge.Update("Tajne Starog grada", 250, newStartLocation, newHiddenPoints);

        challenge.Name.Should().Be("Tajne Starog grada");
        challenge.Xp.Should().Be(250);
        challenge.StartLocation.Should().Be(newStartLocation);
        challenge.HiddenPoints.Should().Equal(newHiddenPoints);
    }

    [Fact]
    public void Moving_the_start_so_that_a_previously_valid_hidden_point_ends_up_farther_than_one_thousand_meters_is_rejected()
    {
        var challenge = CreateChallenge();
        var movedStartLocation = LocationNorthOfStart(-0.0090);

        var update = () => challenge.Update(ValidName, ValidXp, movedStartLocation, challenge.HiddenPoints.ToList());

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Draft, "publish", LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Published, "retire", LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Retired, "archive", LocationChallengeStatus.Archived)]
    [InlineData(LocationChallengeStatus.Retired, "repeat", LocationChallengeStatus.Draft)]
    [InlineData(LocationChallengeStatus.Archived, "repeat", LocationChallengeStatus.Draft)]
    public void Allowed_transition_moves_the_location_challenge_to_the_next_state(
        LocationChallengeStatus status, string action, LocationChallengeStatus expectedStatus)
    {
        var challenge = CreateChallengeIn(status);

        Perform(challenge, action);

        challenge.Status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Published, "publish")]
    [InlineData(LocationChallengeStatus.Retired, "publish")]
    [InlineData(LocationChallengeStatus.Archived, "publish")]
    [InlineData(LocationChallengeStatus.Draft, "retire")]
    [InlineData(LocationChallengeStatus.Retired, "retire")]
    [InlineData(LocationChallengeStatus.Archived, "retire")]
    [InlineData(LocationChallengeStatus.Draft, "archive")]
    [InlineData(LocationChallengeStatus.Published, "archive")]
    [InlineData(LocationChallengeStatus.Archived, "archive")]
    [InlineData(LocationChallengeStatus.Draft, "repeat")]
    [InlineData(LocationChallengeStatus.Published, "repeat")]
    public void Transition_that_is_not_allowed_is_rejected_and_the_location_challenge_keeps_its_state(
        LocationChallengeStatus status, string action)
    {
        var challenge = CreateChallengeIn(status);

        var transition = () => Perform(challenge, action);

        transition.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Archived)]
    public void Repeated_location_challenge_keeps_its_identity_name_xp_start_and_all_hidden_points(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);
        var id = challenge.Id;
        var startLocation = challenge.StartLocation;
        var hiddenPoints = challenge.HiddenPoints.ToList();

        challenge.Repeat();

        challenge.Id.Should().Be(id);
        challenge.Name.Should().Be(ValidName);
        challenge.Xp.Should().Be(ValidXp);
        challenge.StartLocation.Should().Be(startLocation);
        challenge.HiddenPoints.Should().Equal(hiddenPoints);
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Draft)]
    [InlineData(LocationChallengeStatus.Archived)]
    public void Draft_or_archived_location_challenge_can_be_deleted(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.EnsureCanBeDeleted();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Retired)]
    public void Published_or_retired_location_challenge_cannot_be_deleted(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.EnsureCanBeDeleted();

        deletion.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Archived)]
    public void Location_challenge_that_is_not_a_draft_cannot_be_changed(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var update = () => challenge.Update(ValidName, ValidXp, new Location(StartLatitude, StartLongitude), CreateHiddenPoints(1));

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void First_publication_starts_the_first_round()
    {
        var challenge = CreateChallenge();

        challenge.Publish();

        challenge.CurrentRound.Should().Be(1);
    }

    [Fact]
    public void Publication_after_a_repeat_starts_the_next_round()
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Retired);
        challenge.Repeat();

        challenge.Publish();

        challenge.CurrentRound.Should().Be(2);
    }

    // One meter of latitude is about 0.0000089932 degrees, so 0.0000899322 is 10 m, the range boundary.
    [Theory]
    [InlineData(0, true)]
    [InlineData(0.000045, true)]
    [InlineData(0.00008993216, true)]
    [InlineData(0.0000908, false)]
    public void Position_is_within_the_activation_range_only_up_to_ten_meters_from_the_start(double latitudeOffset, bool expected)
    {
        var challenge = CreateChallenge();

        var isWithinRange = challenge.IsWithinActivationRange(LocationNorthOfStart(latitudeOffset));

        isWithinRange.Should().Be(expected);
    }

    // The farthest of the two hidden points is about 111.2 m north of the start, which rounds up to 112 m.
    [Theory]
    [InlineData(0, 112)]
    [InlineData(37, 149)]
    [InlineData(100, 212)]
    public void Search_radius_is_the_distance_to_the_farthest_hidden_point_rounded_up_plus_the_extra_radius(int extraRadiusInMeters, int expected)
    {
        var challenge = CreateChallenge();

        var searchRadius = challenge.SearchRadiusInMeters(extraRadiusInMeters);

        searchRadius.Should().Be(expected);
    }

    [Fact]
    public void Published_location_challenge_can_be_played()
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Published);

        var playing = () => challenge.EnsureCanBePlayed();

        playing.Should().NotThrow();
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Draft)]
    [InlineData(LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Archived)]
    public void Location_challenge_that_is_not_published_cannot_be_played(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var playing = () => challenge.EnsureCanBePlayed();

        playing.Should().Throw<DomainException>();
    }
}
