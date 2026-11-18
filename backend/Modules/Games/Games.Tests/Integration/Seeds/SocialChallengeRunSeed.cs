using Games.Domain.SocialChallengeRuns;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeRunSeed
{
    public static readonly SocialChallengeRun CathedralMeetupEndedYesterdayButStillActive;
    public static readonly SocialChallengeRun PromenadeWalkWithEnoughPlayersAwaitingCheck;
    public static readonly SocialChallengeRun RiverbankSolvedWithFortyXp;

    static SocialChallengeRunSeed()
    {
        // Its end passed yesterday, but no check or activation has written the Expired status yet.
        CathedralMeetupEndedYesterdayButStillActive = new(
            SocialChallengeSeed.PublishedCathedralMeetupOpenNow.Id,
            WellKnownUsers.Explorer,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(-1).AddHours(1));
        CathedralMeetupEndedYesterdayButStillActive.Join(WellKnownUsers.ThirdExplorer, DateTime.UtcNow.AddDays(-1).AddMinutes(5));

        // Explorer's last check lies an hour ahead, so the 20 s rule does not remove that player while the tests run.
        PromenadeWalkWithEnoughPlayersAwaitingCheck = new(
            SocialChallengeSeed.PublishedPromenadeWalkOpenNow.Id,
            WellKnownUsers.ThirdExplorer,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(3));
        PromenadeWalkWithEnoughPlayersAwaitingCheck.Join(WellKnownUsers.Explorer, DateTime.UtcNow.AddHours(1));

        RiverbankSolvedWithFortyXp = new(
            SocialChallengeSeed.RepublishedRiverbankWithMoreXp.Id,
            WellKnownUsers.Explorer,
            new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 11, 10, 11, 0, 0, DateTimeKind.Utc));
        RiverbankSolvedWithFortyXp.Join(WellKnownUsers.ThirdExplorer, new DateTime(2026, 11, 10, 9, 1, 0, DateTimeKind.Utc));
        RiverbankSolvedWithFortyXp.Solve(40, 2, new DateTime(2026, 11, 10, 9, 2, 0, DateTimeKind.Utc));
    }

    public static SocialChallengeRun[] All => [CathedralMeetupEndedYesterdayButStillActive, PromenadeWalkWithEnoughPlayersAwaitingCheck, RiverbankSolvedWithFortyXp];
}
