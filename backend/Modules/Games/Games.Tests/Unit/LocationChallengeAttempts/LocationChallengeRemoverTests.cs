using FluentAssertions;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeAttempts;

public class LocationChallengeRemoverTests
{
    private static readonly DateTime ActivatedAt = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CheckedAt = new(2026, 11, 18, 10, 20, 0, DateTimeKind.Utc);

    private readonly LocationChallengeRemover _remover = new();

    // Published twice, so the challenge is archived in its second round.
    private static LocationChallenge CreateArchivedChallengeInSecondRound()
    {
        var challenge = new LocationChallenge(
            "Skriveni kutci tvrđave",
            100,
            new Location(45.2550, 19.8600),
            [
                new HiddenPoint("/uploads/prva.jpg", new Location(45.2560, 19.8600)),
                new HiddenPoint("/uploads/druga.jpg", new Location(45.2550, 19.8620))
            ]);
        challenge.Publish();
        challenge.Retire();
        challenge.Repeat();
        challenge.Publish();
        challenge.Retire();
        challenge.Archive();
        return challenge;
    }

    private static LocationChallengeAttempt CreateUnfinishedAttempt(LocationChallenge challenge, int round)
    {
        return new LocationChallengeAttempt(challenge.Id, Guid.NewGuid(), round, 200, ActivatedAt);
    }

    private static LocationChallengeAttempt CreateSolvedAttempt(LocationChallenge challenge, int round)
    {
        var attempt = CreateUnfinishedAttempt(challenge, round);
        attempt.RecordDiscovery([1, 2], challenge.HiddenPoints.Count, challenge.Xp, CheckedAt);
        return attempt;
    }

    [Fact]
    public void Challenge_with_an_attempt_solved_in_an_earlier_round_cannot_be_deleted()
    {
        var challenge = CreateArchivedChallengeInSecondRound();
        List<LocationChallengeAttempt> attempts = [CreateSolvedAttempt(challenge, 1), CreateUnfinishedAttempt(challenge, 2)];

        var deletion = () => _remover.EnsureCanBeDeleted(challenge, attempts);

        deletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_with_only_unfinished_attempts_can_be_deleted()
    {
        var challenge = CreateArchivedChallengeInSecondRound();
        List<LocationChallengeAttempt> attempts = [CreateUnfinishedAttempt(challenge, 1), CreateUnfinishedAttempt(challenge, 2)];

        var deletion = () => _remover.EnsureCanBeDeleted(challenge, attempts);

        deletion.Should().NotThrow();
    }
}
