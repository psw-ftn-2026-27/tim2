using FluentAssertions;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeAttempts;

public class LocationChallengeSolverTests
{
    private const int ExtraRadiusInMeters = 30;
    private static readonly Location StartLocation = new(45.2550, 19.8600);
    private static readonly DateTime Now = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);

    private readonly LocationChallengeSolver _solver = new();
    private readonly Guid _playerId = Guid.NewGuid();

    private static LocationChallenge CreatePublishedChallenge()
    {
        var challenge = new LocationChallenge(
            "Skriveni kutci tvrđave",
            100,
            StartLocation,
            [
                new HiddenPoint("/uploads/prva.jpg", new Location(45.2560, 19.8600)),
                new HiddenPoint("/uploads/druga.jpg", new Location(45.2550, 19.8620))
            ]);
        challenge.Publish();
        return challenge;
    }

    private LocationChallengeAttempt Activate(LocationChallenge challenge, LocationChallengeAttempt? currentAttempt)
    {
        return _solver.Activate(challenge, currentAttempt, StartLocation, _playerId, ExtraRadiusInMeters, Now);
    }

    [Fact]
    public void Activation_creates_an_attempt_of_the_current_round_with_the_search_radius_computed_by_the_challenge()
    {
        var challenge = CreatePublishedChallenge();

        var attempt = Activate(challenge, null);

        attempt.ChallengeId.Should().Be(challenge.Id);
        attempt.PlayerId.Should().Be(_playerId);
        attempt.Round.Should().Be(challenge.CurrentRound);
        attempt.SearchRadiusInMeters.Should().Be(challenge.SearchRadiusInMeters(ExtraRadiusInMeters));
        attempt.ActivatedAt.Should().Be(Now);
    }

    [Fact]
    public void Player_with_an_attempt_in_the_current_round_cannot_activate_the_challenge_again()
    {
        var challenge = CreatePublishedChallenge();
        var currentAttempt = Activate(challenge, null);

        var activation = () => Activate(challenge, currentAttempt);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Player_with_an_attempt_from_an_earlier_round_activates_the_challenge_in_the_new_round()
    {
        var challenge = CreatePublishedChallenge();
        var earlierAttempt = Activate(challenge, null);
        challenge.Retire();
        challenge.Repeat();
        challenge.Publish();

        var attempt = Activate(challenge, earlierAttempt);

        attempt.Round.Should().Be(challenge.CurrentRound);
    }
}
