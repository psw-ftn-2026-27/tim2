using FluentAssertions;
using Games.Domain.LocationChallengeAttempts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeAttempts;

public class LocationChallengeAttemptTests
{
    private const int ChallengeXp = 90;
    private static readonly DateTime ActivatedAt = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FirstCheckAt = new(2026, 11, 18, 10, 5, 0, DateTimeKind.Utc);
    private static readonly DateTime SecondCheckAt = new(2026, 11, 18, 10, 9, 0, DateTimeKind.Utc);

    private static LocationChallengeAttempt CreateAttemptWithFirstPointDiscovered(int pointCount)
    {
        var attempt = new LocationChallengeAttempt(Guid.NewGuid(), Guid.NewGuid(), 1, 200, ActivatedAt);
        attempt.RecordDiscovery([1], pointCount, ChallengeXp, FirstCheckAt);
        return attempt;
    }

    [Fact]
    public void Discovery_adds_only_the_points_that_were_not_discovered_yet()
    {
        var attempt = CreateAttemptWithFirstPointDiscovered(4);

        attempt.RecordDiscovery([1, 2, 3], 4, ChallengeXp, SecondCheckAt);

        attempt.DiscoveredPointNumbers.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void Check_without_points_in_range_leaves_the_attempt_unchanged()
    {
        var attempt = CreateAttemptWithFirstPointDiscovered(3);

        attempt.RecordDiscovery([], 3, ChallengeXp, SecondCheckAt);

        attempt.DiscoveredPointNumbers.Should().BeEquivalentTo([1]);
        attempt.SolvedAt.Should().BeNull();
        attempt.EarnedXp.Should().BeNull();
    }

    [Fact]
    public void Attempt_with_undiscovered_points_is_not_solved_and_has_no_earned_xp()
    {
        var attempt = new LocationChallengeAttempt(Guid.NewGuid(), Guid.NewGuid(), 1, 200, ActivatedAt);

        attempt.RecordDiscovery([1], 2, ChallengeXp, FirstCheckAt);

        attempt.SolvedAt.Should().BeNull();
        attempt.EarnedXp.Should().BeNull();
    }

    [Fact]
    public void Discovering_the_last_point_solves_the_attempt_at_the_check_time_with_the_xp_of_the_challenge()
    {
        var attempt = CreateAttemptWithFirstPointDiscovered(2);

        attempt.RecordDiscovery([2], 2, ChallengeXp, SecondCheckAt);

        attempt.SolvedAt.Should().Be(SecondCheckAt);
        attempt.EarnedXp.Should().Be(ChallengeXp);
    }

    [Fact]
    public void Solved_attempt_does_not_accept_a_new_check()
    {
        var attempt = CreateAttemptWithFirstPointDiscovered(1);

        var check = () => attempt.RecordDiscovery([], 1, ChallengeXp, SecondCheckAt);

        check.Should().Throw<DomainException>();
    }
}
