using Games.Domain.Locations;
using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    // A window start is local time in Belgrade, so the seeds read the current local time (to the minute)
    // to build windows that are open or closed while the tests run.
    private static readonly DateTime BelgradeNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Europe/Belgrade");
    private static readonly TimeOnly BelgradeTimeNow = new(BelgradeNow.Hour, BelgradeNow.Minute);

    public static readonly SocialChallenge DraftMorningRun = new("Jutarnje trčanje na keju", new TimeOnly(7, 0), TimeSpan.FromHours(1), 5, 200, 50, "Bravo, istrčali ste jutro zajedno!", new Location(45.2510, 19.8530));
    public static readonly SocialChallenge DraftNightGathering = new("Ponoćno okupljanje na tvrđavi", new TimeOnly(22, 0), TimeSpan.FromHours(4), 10, 500, 120, "Dočekali ste ponoć na tvrđavi.", new Location(45.2525, 19.8620));
    public static readonly SocialChallenge DraftSquareMeetup = new("Susret na trgu", new TimeOnly(12, 30), TimeSpan.FromMinutes(45), 3, 50, 20, "Upoznali ste nove istraživače.", null);
    public static readonly SocialChallenge PublishedParkPicnic;
    public static readonly SocialChallenge RetiredBridgeWalk;
    public static readonly SocialChallenge ArchivedMarketVisit;
    public static readonly SocialChallenge PublishedSquareGatheringOpenNow;
    public static readonly SocialChallenge PublishedCathedralMeetupOpenNow;
    public static readonly SocialChallenge PublishedPromenadeWalkOpenNow;
    public static readonly SocialChallenge PublishedEveningConcertClosedNow;
    public static readonly SocialChallenge RepublishedRiverbankWithMoreXp;

    static SocialChallengeSeed()
    {
        PublishedParkPicnic = new("Piknik u Dunavskom parku", new TimeOnly(16, 0), TimeSpan.FromHours(2), 6, 100, 40, "Uživali ste u pikniku zajedno.", new Location(45.2575, 19.8510));
        PublishedParkPicnic.Publish();

        RetiredBridgeWalk = new("Šetnja preko mosta", new TimeOnly(18, 0), TimeSpan.FromMinutes(30), 4, 300, 25, "Prešli ste most zajedno.", new Location(45.2490, 19.8590));
        RetiredBridgeWalk.Publish();
        RetiredBridgeWalk.Retire();

        ArchivedMarketVisit = new("Jutro na pijaci", new TimeOnly(8, 0), TimeSpan.FromMinutes(90), 3, 80, 15, "Obišli ste pijacu zajedno.", new Location(45.2560, 19.8400));
        ArchivedMarketVisit.Publish();
        ArchivedMarketVisit.Retire();
        ArchivedMarketVisit.Archive();

        // The next four stand where ThirdExplorer's seeded position is (the Synagogue), so that player is in their range.
        PublishedSquareGatheringOpenNow = new("Okupljanje kod Sinagoge", BelgradeTimeNow.AddHours(-1), TimeSpan.FromHours(3), 3, 50, 30, "Okupili ste se kod Sinagoge.", new Location(45.2559, 19.8413));
        PublishedSquareGatheringOpenNow.Publish();

        PublishedCathedralMeetupOpenNow = new("Susret kod katedrale", BelgradeTimeNow.AddHours(-1), TimeSpan.FromHours(3), 3, 50, 35, "Sreli ste se kod katedrale.", new Location(45.2559, 19.8413));
        PublishedCathedralMeetupOpenNow.Publish();

        PublishedPromenadeWalkOpenNow = new("Zajednička šetnja promenadom", BelgradeTimeNow.AddHours(-1), TimeSpan.FromHours(3), 2, 50, 45, "Prošetali ste promenadom zajedno.", new Location(45.2559, 19.8413));
        PublishedPromenadeWalkOpenNow.Publish();

        PublishedEveningConcertClosedNow = new("Koncert u dvorištu", BelgradeTimeNow.AddHours(2), TimeSpan.FromHours(1), 4, 50, 60, "Slušali ste koncert zajedno.", new Location(45.2559, 19.8413));
        PublishedEveningConcertClosedNow.Publish();

        // Solved by a run while it carried 40 XP; it was later repeated, changed to 80 XP and published again.
        RepublishedRiverbankWithMoreXp = new("Druženje na obali", new TimeOnly(10, 0), TimeSpan.FromHours(2), 2, 100, 40, "Družili ste se na obali.", new Location(45.2520, 19.8550));
        RepublishedRiverbankWithMoreXp.Publish();
        RepublishedRiverbankWithMoreXp.Retire();
        RepublishedRiverbankWithMoreXp.Repeat();
        RepublishedRiverbankWithMoreXp.Update(
            RepublishedRiverbankWithMoreXp.Name,
            RepublishedRiverbankWithMoreXp.Window.Start,
            RepublishedRiverbankWithMoreXp.Window.Duration,
            RepublishedRiverbankWithMoreXp.RequiredParticipants,
            RepublishedRiverbankWithMoreXp.RangeInMeters,
            80,
            RepublishedRiverbankWithMoreXp.CompletionMessage,
            RepublishedRiverbankWithMoreXp.Location);
        RepublishedRiverbankWithMoreXp.Publish();
    }

    public static SocialChallenge[] All => [DraftMorningRun, DraftNightGathering, DraftSquareMeetup, PublishedParkPicnic, RetiredBridgeWalk, ArchivedMarketVisit, PublishedSquareGatheringOpenNow, PublishedCathedralMeetupOpenNow, PublishedPromenadeWalkOpenNow, PublishedEveningConcertClosedNow, RepublishedRiverbankWithMoreXp];
}
