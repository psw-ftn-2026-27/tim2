using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftMorningRun = new("Jutarnje trčanje na keju", new TimeOnly(7, 0), TimeSpan.FromHours(1), 5, 200, 50, "Bravo, istrčali ste jutro zajedno!");
    public static readonly SocialChallenge DraftNightGathering = new("Ponoćno okupljanje na tvrđavi", new TimeOnly(22, 0), TimeSpan.FromHours(4), 10, 500, 120, "Dočekali ste ponoć na tvrđavi.");
    public static readonly SocialChallenge DraftSquareMeetup = new("Susret na trgu", new TimeOnly(12, 30), TimeSpan.FromMinutes(45), 3, 50, 20, "Upoznali ste nove istraživače.");
    public static readonly SocialChallenge PublishedParkPicnic;
    public static readonly SocialChallenge RetiredBridgeWalk;
    public static readonly SocialChallenge ArchivedMarketVisit;

    static SocialChallengeSeed()
    {
        PublishedParkPicnic = new("Piknik u Dunavskom parku", new TimeOnly(16, 0), TimeSpan.FromHours(2), 6, 100, 40, "Uživali ste u pikniku zajedno.");
        PublishedParkPicnic.Publish();

        RetiredBridgeWalk = new("Šetnja preko mosta", new TimeOnly(18, 0), TimeSpan.FromMinutes(30), 4, 300, 25, "Prešli ste most zajedno.");
        RetiredBridgeWalk.Publish();
        RetiredBridgeWalk.Retire();

        ArchivedMarketVisit = new("Jutro na pijaci", new TimeOnly(8, 0), TimeSpan.FromMinutes(90), 3, 80, 15, "Obišli ste pijacu zajedno.");
        ArchivedMarketVisit.Publish();
        ArchivedMarketVisit.Retire();
        ArchivedMarketVisit.Archive();
    }

    public static SocialChallenge[] All => [DraftMorningRun, DraftNightGathering, DraftSquareMeetup, PublishedParkPicnic, RetiredBridgeWalk, ArchivedMarketVisit];
}
