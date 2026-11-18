using FluentAssertions;
using Games.Domain.Locations;
using Games.Domain.SocialChallengeRuns;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallengeRuns;

public class SocialChallengeSolverTests
{
    private const double ChallengeLatitude = 45;
    private const double ChallengeLongitude = 19;
    private const int ChallengeRangeInMeters = 100;
    private const int ChallengeXp = 50;
    private const double FarLatitude = 46;

    // 11:00 in Belgrade (winter time, UTC+1), inside the default window from 10:00 to 14:00 local time.
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private static readonly TimeOnly DefaultStart = new(10, 0);
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(4);
    private static readonly DateTime DefaultEnd = new(2026, 1, 15, 13, 0, 0, DateTimeKind.Utc);

    private readonly SocialChallengeSolver _solver = new();
    private readonly Guid _firstPlayer = Guid.NewGuid();
    private readonly Guid _secondPlayer = Guid.NewGuid();
    private readonly Guid _thirdPlayer = Guid.NewGuid();

    private static SocialChallenge CreateDraftChallenge(TimeOnly start, TimeSpan duration, int requiredParticipants)
    {
        return new SocialChallenge(
            "Okupljanje na trgu",
            start,
            duration,
            requiredParticipants,
            ChallengeRangeInMeters,
            ChallengeXp,
            "Uspeli ste zajedno!",
            new Location(ChallengeLatitude, ChallengeLongitude));
    }

    private static SocialChallenge CreatePublishedChallenge(int requiredParticipants)
    {
        return CreatePublishedChallenge(DefaultStart, DefaultDuration, requiredParticipants);
    }

    private static SocialChallenge CreatePublishedChallenge(TimeOnly start, TimeSpan duration, int requiredParticipants)
    {
        var challenge = CreateDraftChallenge(start, duration, requiredParticipants);
        challenge.Publish();
        return challenge;
    }

    private SocialChallengeRun StartRun(SocialChallenge challenge, Guid playerId, DateTime now)
    {
        return _solver.Activate(challenge, null, null, playerId, ChallengeLatitude, ChallengeLongitude, now)!;
    }

    private void Join(SocialChallenge challenge, SocialChallengeRun run, Guid playerId, DateTime now)
    {
        _solver.Activate(challenge, run, null, playerId, ChallengeLatitude, ChallengeLongitude, now);
    }

    private SocialChallengeRun CreateSolvedRun(SocialChallenge challenge)
    {
        var run = StartRun(challenge, _firstPlayer, Now);
        Join(challenge, run, _secondPlayer, Now.AddSeconds(5));
        _solver.Check(challenge, run, _secondPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddSeconds(10));
        return run;
    }

    [Fact]
    public void Participant_without_a_check_for_more_than_twenty_seconds_is_removed_before_the_participants_are_counted()
    {
        var challenge = CreatePublishedChallenge(2);
        var run = StartRun(challenge, _firstPlayer, Now);
        Join(challenge, run, _secondPlayer, Now.AddSeconds(10));

        _solver.Check(challenge, run, _secondPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddSeconds(21));

        run.Status.Should().Be(SocialChallengeRunStatus.Active);
        run.Participants.Select(participant => participant.PlayerId).Should().Equal(_secondPlayer);
    }

    [Fact]
    public void Check_that_finds_enough_participants_solves_the_run_for_everyone_in_it_with_the_challenge_xp()
    {
        var challenge = CreatePublishedChallenge(2);
        var run = StartRun(challenge, _firstPlayer, Now);
        Join(challenge, run, _secondPlayer, Now.AddSeconds(5));

        _solver.Check(challenge, run, _secondPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddSeconds(10));

        run.Status.Should().Be(SocialChallengeRunStatus.Solved);
        run.SolvedAt.Should().Be(Now.AddSeconds(10));
        run.EarnedXp.Should().Be(ChallengeXp);
        run.Participants.Select(participant => participant.PlayerId).Should().BeEquivalentTo([_firstPlayer, _secondPlayer]);
    }

    [Theory]
    [InlineData(FarLatitude, ChallengeLongitude)]
    [InlineData(null, null)]
    public void Player_outside_the_range_or_without_a_saved_position_is_removed_from_the_run_at_a_check(double? latitude, double? longitude)
    {
        var challenge = CreatePublishedChallenge(3);
        var run = StartRun(challenge, _firstPlayer, Now);
        Join(challenge, run, _secondPlayer, Now.AddSeconds(5));

        _solver.Check(challenge, run, _firstPlayer, latitude, longitude, Now.AddSeconds(10));

        run.HasParticipant(_firstPlayer).Should().BeFalse();
        run.HasParticipant(_secondPlayer).Should().BeTrue();
    }

    [Fact]
    public void Run_past_its_end_expires_and_is_not_solved_even_with_enough_participants()
    {
        var challenge = CreatePublishedChallenge(2);
        var run = StartRun(challenge, _firstPlayer, Now);
        Join(challenge, run, _secondPlayer, Now.AddSeconds(5));

        _solver.Check(challenge, run, _secondPlayer, ChallengeLatitude, ChallengeLongitude, run.EndsAt);

        run.Status.Should().Be(SocialChallengeRunStatus.Expired);
        run.SolvedAt.Should().BeNull();
        run.EarnedXp.Should().BeNull();
    }

    [Fact]
    public void Second_player_who_activates_the_challenge_joins_the_active_run_instead_of_starting_a_new_one()
    {
        var challenge = CreatePublishedChallenge(3);
        var run = StartRun(challenge, _firstPlayer, Now);

        var newRun = _solver.Activate(challenge, run, null, _secondPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddSeconds(5));

        newRun.Should().BeNull();
        run.Participants.Select(participant => participant.PlayerId).Should().Equal(_firstPlayer, _secondPlayer);
    }

    [Fact]
    public void Player_from_a_solved_run_cannot_activate_the_challenge_again()
    {
        var challenge = CreatePublishedChallenge(2);
        var solvedRun = CreateSolvedRun(challenge);

        var activation = () => _solver.Activate(challenge, null, solvedRun, _firstPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddMinutes(1));

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Player_removed_from_the_run_can_activate_the_challenge_again()
    {
        var challenge = CreatePublishedChallenge(3);
        var run = StartRun(challenge, _firstPlayer, Now);
        Join(challenge, run, _secondPlayer, Now.AddSeconds(5));
        _solver.Check(challenge, run, _firstPlayer, null, null, Now.AddSeconds(10));

        var newRun = _solver.Activate(challenge, run, null, _firstPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddSeconds(15));

        newRun.Should().BeNull();
        run.HasParticipant(_firstPlayer).Should().BeTrue();
    }

    [Fact]
    public void Player_exactly_on_the_range_boundary_activates_the_challenge()
    {
        var challenge = CreatePublishedChallenge(3);

        // About 100 m north of the challenge location, which is exactly the range.
        var run = _solver.Activate(challenge, null, null, _firstPlayer, 45.0008993216, ChallengeLongitude, Now);

        run.Should().NotBeNull();
        run!.HasParticipant(_firstPlayer).Should().BeTrue();
    }

    [Fact]
    public void Player_one_meter_beyond_the_range_boundary_cannot_activate_the_challenge()
    {
        var challenge = CreatePublishedChallenge(3);

        // About 101 m north of the challenge location.
        var activation = () => _solver.Activate(challenge, null, null, _firstPlayer, 45.0009083148, ChallengeLongitude, Now);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Player_activates_the_challenge_exactly_when_the_window_starts_and_the_run_ends_with_the_window()
    {
        // 16:00 to 18:00 in Belgrade in summer time (UTC+2) is 14:00 to 16:00 UTC.
        var challenge = CreatePublishedChallenge(new TimeOnly(16, 0), TimeSpan.FromHours(2), 3);
        var windowStart = new DateTime(2026, 7, 15, 14, 0, 0, DateTimeKind.Utc);

        var run = _solver.Activate(challenge, null, null, _firstPlayer, ChallengeLatitude, ChallengeLongitude, windowStart);

        run!.StartedAt.Should().Be(windowStart);
        run.EndsAt.Should().Be(new DateTime(2026, 7, 15, 16, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Player_cannot_activate_the_challenge_once_the_window_has_ended()
    {
        var challenge = CreatePublishedChallenge(new TimeOnly(16, 0), TimeSpan.FromHours(2), 3);
        var windowEnd = new DateTime(2026, 7, 15, 16, 0, 0, DateTimeKind.Utc);

        var activation = () => _solver.Activate(challenge, null, null, _firstPlayer, ChallengeLatitude, ChallengeLongitude, windowEnd);

        activation.Should().Throw<DomainException>();
    }

    // The window runs from 23:30 on 15 January to 00:30 on 16 January in Belgrade (winter time, UTC+1),
    // so 23:45 local is 22:45 UTC and 00:15 local is 23:15 UTC; both runs end at 00:30 on 16 January local, 23:30 UTC.
    [Theory]
    [InlineData(22, 45)]
    [InlineData(23, 15)]
    public void Window_that_crosses_midnight_accepts_an_activation_on_both_sides_and_the_run_ends_at_the_window_end_after_midnight(int hour, int minute)
    {
        var challenge = CreatePublishedChallenge(new TimeOnly(23, 30), TimeSpan.FromHours(1), 3);
        var activatedAt = new DateTime(2026, 1, 15, hour, minute, 0, DateTimeKind.Utc);

        var run = _solver.Activate(challenge, null, null, _firstPlayer, ChallengeLatitude, ChallengeLongitude, activatedAt);

        run!.EndsAt.Should().Be(new DateTime(2026, 1, 15, 23, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Activation_after_a_solved_run_starts_a_new_run()
    {
        var challenge = CreatePublishedChallenge(2);
        var solvedRun = CreateSolvedRun(challenge);

        var newRun = _solver.Activate(challenge, solvedRun, null, _thirdPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddMinutes(1));

        newRun.Should().NotBeNull();
        newRun!.Participants.Select(participant => participant.PlayerId).Should().Equal(_thirdPlayer);
        solvedRun.HasParticipant(_thirdPlayer).Should().BeFalse();
    }

    [Fact]
    public void Activation_after_the_active_run_has_ended_closes_it_and_starts_a_new_run()
    {
        var challenge = CreatePublishedChallenge(3);
        var endedRun = StartRun(challenge, _firstPlayer, Now);

        var newRun = _solver.Activate(challenge, endedRun, null, _secondPlayer, ChallengeLatitude, ChallengeLongitude, Now.AddDays(1));

        endedRun.Status.Should().Be(SocialChallengeRunStatus.Expired);
        newRun.Should().NotBeNull();
        newRun!.EndsAt.Should().Be(DefaultEnd.AddDays(1));
    }

    [Fact]
    public void Check_by_a_participant_of_a_solved_run_changes_nothing()
    {
        var challenge = CreatePublishedChallenge(2);
        var solvedRun = CreateSolvedRun(challenge);

        _solver.Check(challenge, solvedRun, _firstPlayer, null, null, Now.AddMinutes(1));

        solvedRun.Status.Should().Be(SocialChallengeRunStatus.Solved);
        solvedRun.SolvedAt.Should().Be(Now.AddSeconds(10));
        solvedRun.Participants.Select(participant => participant.PlayerId).Should().BeEquivalentTo([_firstPlayer, _secondPlayer]);
    }

    [Fact]
    public void Social_challenge_that_is_not_published_cannot_be_activated()
    {
        var challenge = CreateDraftChallenge(DefaultStart, DefaultDuration, 3);

        var activation = () => _solver.Activate(challenge, null, null, _firstPlayer, ChallengeLatitude, ChallengeLongitude, Now);

        activation.Should().Throw<DomainException>();
    }
}
