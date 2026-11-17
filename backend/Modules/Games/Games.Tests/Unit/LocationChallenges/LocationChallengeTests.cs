using FluentAssertions;
using Games.Domain.LocationChallenges;
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
}
