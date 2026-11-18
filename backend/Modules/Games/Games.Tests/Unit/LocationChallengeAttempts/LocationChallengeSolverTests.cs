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

    private LocationChallengeAttempt Activate(LocationChallenge challenge, LocationChallengeAttempt? attemptInCurrentRound)
    {
        return _solver.Activate(challenge, attemptInCurrentRound, StartLocation, _playerId, ExtraRadiusInMeters, Now);
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
        var attemptInCurrentRound = Activate(challenge, null);

        var activation = () => Activate(challenge, attemptInCurrentRound);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Player_without_an_attempt_in_the_new_round_of_a_republished_challenge_activates_it_in_that_round()
    {
        var challenge = CreatePublishedChallenge();
        var earlierAttempt = Activate(challenge, null);
        challenge.Retire();
        challenge.Repeat();
        challenge.Publish();

        var attempt = Activate(challenge, null);

        attempt.Round.Should().Be(challenge.CurrentRound).And.BeGreaterThan(earlierAttempt.Round);
    }
}
