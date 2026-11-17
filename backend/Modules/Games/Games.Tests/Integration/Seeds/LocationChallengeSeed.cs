using Games.Domain.LocationChallenges;
using Games.Domain.Locations;

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

    public static readonly LocationChallenge PublishedStrandBeach;
    public static readonly LocationChallenge RetiredFishermansIsland;
    public static readonly LocationChallenge ArchivedFortressTunnels;

    static LocationChallengeSeed()
    {
        PublishedStrandBeach = new(
            "Plaža Štrand",
            60,
            new Location(45.2440, 19.8500),
            [
                new HiddenPoint("/uploads/strand-tusevi.jpg", new Location(45.2450, 19.8500)),
                new HiddenPoint("/uploads/strand-kapija.jpg", new Location(45.2440, 19.8520))
            ]);
        PublishedStrandBeach.Publish();

        RetiredFishermansIsland = new(
            "Ribarsko ostrvo",
            90,
            new Location(45.2380, 19.8700),
            [
                new HiddenPoint("/uploads/ribarac-camci.jpg", new Location(45.2390, 19.8700)),
                new HiddenPoint("/uploads/ribarac-mreze.jpg", new Location(45.2380, 19.8720))
            ]);
        RetiredFishermansIsland.Publish();
        RetiredFishermansIsland.Retire();

        ArchivedFortressTunnels = new(
            "Podzemni hodnici tvrđave",
            150,
            new Location(45.2510, 19.8630),
            [
                new HiddenPoint("/uploads/hodnici-ulaz.jpg", new Location(45.2520, 19.8630)),
                new HiddenPoint("/uploads/hodnici-bunar.jpg", new Location(45.2510, 19.8650)),
                new HiddenPoint("/uploads/hodnici-svod.jpg", new Location(45.2500, 19.8620))
            ]);
        ArchivedFortressTunnels.Publish();
        ArchivedFortressTunnels.Retire();
        ArchivedFortressTunnels.Archive();
    }

    public static LocationChallenge[] All => [DraftQuayByTheBridge, DraftOldTownSecrets, PublishedStrandBeach, RetiredFishermansIsland, ArchivedFortressTunnels];
}
