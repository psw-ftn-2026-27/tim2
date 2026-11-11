using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftMorningRun = new("Jutarnje trčanje na keju", new TimeOnly(7, 0), TimeSpan.FromHours(1), 5, 200, 50, "Bravo, istrčali ste jutro zajedno!");
    public static readonly SocialChallenge DraftNightGathering = new("Ponoćno okupljanje na tvrđavi", new TimeOnly(22, 0), TimeSpan.FromHours(4), 10, 500, 120, "Dočekali ste ponoć na tvrđavi.");
    public static readonly SocialChallenge DraftSquareMeetup = new("Susret na trgu", new TimeOnly(12, 30), TimeSpan.FromMinutes(45), 3, 50, 20, "Upoznali ste nove istraživače.");

    public static SocialChallenge[] All => [DraftMorningRun, DraftNightGathering, DraftSquareMeetup];
}
