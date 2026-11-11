using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"]);
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"]);
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"]);
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour PublishedLakeside;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"]);
        PublishableRiverside.ChangeRecommendedGroupSize(2, 10);
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.ChangePrice(1500);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"]);
        PublishedVineyards.ChangeRecommendedGroupSize(4, 12);
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.Publish();

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"]);
        PublishedMonasteries.ChangeRecommendedGroupSize(1, 8);
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.Publish();

        PublishedLakeside = new(WellKnownUsers.SecondExplorer, "Staza oko Palićkog jezera", "Staza obilazi Palićko jezero od Velike terase do Ženskog štranda, pored Vodotornja i Muzičkog paviljona, uz odmor u parku kraj obale.", TourDifficulty.Easy, ["priroda", "jezero"]);
        PublishedLakeside.ChangeRecommendedGroupSize(2, 20);
        PublishedLakeside.AddTransportTime(TransportMode.Walking, 120);
        PublishedLakeside.Publish();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, PublishedLakeside];
}
