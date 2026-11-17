using Games.Domain.LocationChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeSeed
{
    public static readonly LocationChallenge DraftQuayByTheBridge = new(
        "Kej kod Varadinskog mosta",
        70,
        new Location(45.2550, 19.8600),
        [
            new HiddenPoint("/uploads/kej-klupa.jpg", new Location(45.2560, 19.8600)),
            new HiddenPoint("/uploads/kej-stub.jpg", new Location(45.2550, 19.8620))
        ]);

    public static readonly LocationChallenge DraftOldTownSecrets = new(
        "Tajne Starog grada",
        120,
        new Location(45.2551, 19.8451),
        [
            new HiddenPoint("/uploads/stari-grad-portal.jpg", new Location(45.2565, 19.8451)),
            new HiddenPoint("/uploads/stari-grad-fasada.jpg", new Location(45.2551, 19.8475)),
            new HiddenPoint("/uploads/stari-grad-dvoriste.jpg", new Location(45.2540, 19.8440))
        ]);

    public static LocationChallenge[] All => [DraftQuayByTheBridge, DraftOldTownSecrets];
}
