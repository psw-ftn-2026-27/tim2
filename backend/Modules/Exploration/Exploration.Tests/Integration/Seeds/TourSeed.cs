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
    public static readonly Tour PublishedFreeParkWalk;
    public static readonly Tour PublishedCheapGallery;
    public static readonly Tour PublishedModeratelyPricedFortress;
    public static readonly Tour PublishedExpensiveMountain;
    public static readonly Tour ArchivedCastle;
    public static readonly Tour DraftOldTownWithThreeKeyPoints;
    public static readonly Tour SecondExplorersDraftMarketWithTwoKeyPoints;
    public static readonly Tour PublishedBridgesWithTwoKeyPoints;
    public static readonly Tour PublishedKalemegdanWithVisibleKeyPointInBelgrade;
    public static readonly Tour PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade;
    public static readonly Tour PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade;
    public static readonly Tour DraftDorcolWithVisibleKeyPointInBelgrade;
    public static readonly Tour ArchivedTasmajdanWithVisibleKeyPointInBelgrade;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"]);
        PublishableRiverside.ChangeRecommendedGroupSize(2, 10);
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.ChangePrice(1500);
        PublishableRiverside.AddKeyPoint("Ribarsko ostrvo", "Polazna tačka kod ulaza na Ribarsko ostrvo.", "/images/ribarsko-ostrvo.jpg", new Location(45.2381, 19.8742), null, true);
        PublishableRiverside.AddKeyPoint("Štrand", "Gradska plaža na obali Dunava.", "/images/strand.jpg", new Location(45.2440, 19.8530), null, true);
        PublishableRiverside.AddKeyPoint("Most slobode", "Završetak staze kod viseće konstrukcije mosta.", "/images/most-slobode-kraj.jpg", new Location(45.2390, 19.8290), "Ispod mosta je mirno mesto za predah.", false);
        PublishableRiverside.SetCalculatedLength(5.6);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"]);
        PublishedVineyards.ChangeRecommendedGroupSize(4, 12);
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.AddKeyPoint("Trg Branka Radičevića", "Centralni trg Sremskih Karlovaca sa česmom Četiri lava.", "/images/karlovci-trg.jpg", new Location(45.2027, 19.9342), null, true);
        PublishedVineyards.AddKeyPoint("Porodični podrum", "Vinski podrum sa degustacijom bermeta.", "/images/bermet-podrum.jpg", new Location(45.1986, 19.9301), "Najstarija buradi su u zadnjem delu podruma.", false);
        PublishedVineyards.SetCalculatedLength(4.2);
        PublishedVineyards.Publish();

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"]);
        PublishedMonasteries.ChangeRecommendedGroupSize(1, 8);
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.AddKeyPoint("Manastir Krušedol", "Zadužbina porodice Branković.", "/images/krusedol.jpg", new Location(45.1185, 19.9473), null, true);
        PublishedMonasteries.AddKeyPoint("Manastir Grgeteg", "Manastir na južnim padinama Fruške gore.", "/images/grgeteg.jpg", new Location(45.1394, 19.8935), null, true);
        PublishedMonasteries.AddKeyPoint("Manastir Novo Hopovo", "Manastir sa crkvom iz šesnaestog veka.", "/images/novo-hopovo.jpg", new Location(45.1478, 19.8017), "U porti raste stara lipa.", false);
        PublishedMonasteries.SetCalculatedLength(21.3);
        PublishedMonasteries.Publish();

        PublishedLakeside = new(WellKnownUsers.SecondExplorer, "Staza oko Palićkog jezera", "Staza obilazi Palićko jezero od Velike terase do Ženskog štranda, pored Vodotornja i Muzičkog paviljona, uz odmor u parku kraj obale.", TourDifficulty.Easy, ["priroda", "jezero"]);
        PublishedLakeside.ChangeRecommendedGroupSize(2, 20);
        PublishedLakeside.AddTransportTime(TransportMode.Walking, 120);
        PublishedLakeside.AddKeyPoint("Velika terasa", "Polazna tačka na obali jezera.", "/images/velika-terasa.jpg", new Location(46.1005, 19.7633), null, true);
        PublishedLakeside.AddKeyPoint("Ženski štrand", "Plaža na severnoj obali jezera.", "/images/zenski-strand.jpg", new Location(46.1037, 19.7676), null, true);
        PublishedLakeside.SetCalculatedLength(3.1);
        PublishedLakeside.Publish();

        PublishedFreeParkWalk = new(WellKnownUsers.SecondExplorer, "Šetnja Dunavskim parkom", "Šetnja vodi kroz Dunavski park, od jezerceta sa ostrvom i labudovima, pored spomenika Đuri Jakšiću, do izlaza ka Zmaj Jovinoj ulici.", TourDifficulty.Easy, ["park"]);
        PublishedFreeParkWalk.ChangeRecommendedGroupSize(1, 15);
        PublishedFreeParkWalk.AddTransportTime(TransportMode.Walking, 40);
        PublishedFreeParkWalk.ChangePrice(0);
        PublishedFreeParkWalk.AddKeyPoint("Jezerce sa ostrvom", "Jezerce sa labudovima u sredini parka.", "/images/jezerce.jpg", new Location(45.2533, 19.8506), null, true);
        PublishedFreeParkWalk.AddKeyPoint("Spomenik Đuri Jakšiću", "Spomenik pesniku u severnom delu parka.", "/images/djura-jaksic.jpg", new Location(45.2545, 19.8491), null, true);
        PublishedFreeParkWalk.SetCalculatedLength(0.6);
        PublishedFreeParkWalk.Publish();

        PublishedCheapGallery = new(WellKnownUsers.SecondExplorer, "Obilazak galerija", "Obilazak počinje u Galeriji Matice srpske, nastavlja se kroz Spomen-zbirku Pavla Beljanskog i završava u Galeriji likovne umetnosti poklon-zbirci Rajka Mamuzića.", TourDifficulty.Easy, ["umetnost"]);
        PublishedCheapGallery.ChangeRecommendedGroupSize(1, 10);
        PublishedCheapGallery.AddTransportTime(TransportMode.Walking, 75);
        PublishedCheapGallery.ChangePrice(500);
        PublishedCheapGallery.AddKeyPoint("Galerija Matice srpske", "Zbirka srpske umetnosti od osamnaestog veka.", "/images/galerija-matice.jpg", new Location(45.2558, 19.8496), null, true);
        PublishedCheapGallery.AddKeyPoint("Spomen-zbirka Pavla Beljanskog", "Zbirka srpske umetnosti dvadesetog veka.", "/images/beljanski.jpg", new Location(45.2563, 19.8487), null, true);
        PublishedCheapGallery.SetCalculatedLength(0.4);
        PublishedCheapGallery.Publish();

        PublishedModeratelyPricedFortress = new(WellKnownUsers.SecondExplorer, "Podzemlje tvrđave", "Obilazak vodi kroz podzemne vojne galerije Petrovaradinske tvrđave, sa vodičem i fenjerima, uz priču o odbrani tvrđave i njenim graditeljima.", TourDifficulty.Moderate, ["grad"]);
        PublishedModeratelyPricedFortress.ChangeRecommendedGroupSize(2, 8);
        PublishedModeratelyPricedFortress.AddTransportTime(TransportMode.Car, 30);
        PublishedModeratelyPricedFortress.ChangePrice(1200);
        PublishedModeratelyPricedFortress.AddKeyPoint("Ulaz u galerije", "Mesto gde vodič deli fenjere.", "/images/ulaz-galerije.jpg", new Location(45.2508, 19.8649), null, true);
        PublishedModeratelyPricedFortress.AddKeyPoint("Podzemni hodnik", "Najduži hodnik ispod tvrđave.", "/images/podzemni-hodnik.jpg", new Location(45.2515, 19.8630), "U hodniku se čuje odjek koraka sa svih nivoa.", false);
        PublishedModeratelyPricedFortress.SetCalculatedLength(1.2);
        PublishedModeratelyPricedFortress.Publish();

        PublishedExpensiveMountain = new(WellKnownUsers.SecondExplorer, "Uspon na Crveni čot", "Celodnevni uspon na Crveni čot, najviši vrh Fruške gore, kreće iz Iriškog venca, prolazi pored TV tornja i vraća se šumskim putem do polazne tačke.", TourDifficulty.Hard, ["planina"]);
        PublishedExpensiveMountain.ChangeRecommendedGroupSize(2, 6);
        PublishedExpensiveMountain.AddTransportTime(TransportMode.Car, 50);
        PublishedExpensiveMountain.ChangePrice(3000);
        PublishedExpensiveMountain.AddKeyPoint("Iriški venac", "Polazna tačka kod spomenika.", "/images/iriski-venac.jpg", new Location(45.1497, 19.8603), null, true);
        PublishedExpensiveMountain.AddKeyPoint("Crveni čot", "Najviši vrh Fruške gore.", "/images/crveni-cot.jpg", new Location(45.1564, 19.7186), "Sa vrha se po vedrom danu vidi Avala.", false);
        PublishedExpensiveMountain.SetCalculatedLength(14.8);
        PublishedExpensiveMountain.Publish();

        ArchivedCastle = new(WellKnownUsers.Explorer, "Dvorac Fantast", "Obilazak dvorca Fantast kod Bečeja vodi kroz kapelu, park i konjušnicu, uz priču o porodici Dunđerski i njihovoj ergeli, sa šetnjom do obližnjeg jezera.", TourDifficulty.Easy, ["istorija"]);
        ArchivedCastle.ChangeRecommendedGroupSize(2, 15);
        ArchivedCastle.AddTransportTime(TransportMode.Car, 40);
        ArchivedCastle.AddKeyPoint("Kapela", "Kapela uz dvorac Fantast.", "/images/fantast-kapela.jpg", new Location(45.6247, 20.0617), null, true);
        ArchivedCastle.AddKeyPoint("Konjušnica", "Konjušnica nekadašnje ergele Dunđerskih.", "/images/fantast-konjusnica.jpg", new Location(45.6232, 20.0638), null, true);
        ArchivedCastle.SetCalculatedLength(0.8);
        ArchivedCastle.Publish();
        ArchivedCastle.Archive();

        DraftOldTownWithThreeKeyPoints = new(WellKnownUsers.Explorer, "Stari grad Sombora", "Kratak opis šetnje kroz stari grad.", TourDifficulty.Easy, ["grad"]);
        DraftOldTownWithThreeKeyPoints.AddKeyPoint("Gradska kuća", "Zgrada gradske kuće na glavnom trgu.", "/images/gradska-kuca.jpg", new Location(45.7733, 19.1151), null, true);
        DraftOldTownWithThreeKeyPoints.AddKeyPoint("Županija", "Zgrada Županije sa najvećom slikom u zemlji.", "/images/zupanija.jpg", new Location(45.7745, 19.1123), "Slika Bitka kod Sente zauzima ceo zid sale.", false);
        DraftOldTownWithThreeKeyPoints.AddKeyPoint("Karmelićanski samostan", "Samostan u mirnom delu centra.", "/images/samostan.jpg", new Location(45.7722, 19.1170), "Ispod samostana postoje stare podrumske prostorije.", false);

        SecondExplorersDraftMarketWithTwoKeyPoints = new(WellKnownUsers.SecondExplorer, "Pijaca i okolina", "Kratak opis obilaska pijace.", TourDifficulty.Easy, ["grad"]);
        SecondExplorersDraftMarketWithTwoKeyPoints.AddKeyPoint("Ulaz u pijacu", "Glavni ulaz sa tezgama voća.", "/images/pijaca-ulaz.jpg", new Location(45.2551, 19.8451), null, true);
        SecondExplorersDraftMarketWithTwoKeyPoints.AddKeyPoint("Riblja pijaca", "Deo pijace sa ribom sa Dunava.", "/images/riblja-pijaca.jpg", new Location(45.2559, 19.8462), "Najbolja riba stiže pre sedam ujutru.", false);

        PublishedBridgesWithTwoKeyPoints = new(WellKnownUsers.Explorer, "Mostovi Novog Sada", "Obilazak vodi duž keja od Varadinskog mosta do Mosta slobode, uz priču o tome kako su mostovi rušeni i ponovo građeni tokom istorije grada.", TourDifficulty.Hard, ["most"]);
        PublishedBridgesWithTwoKeyPoints.ChangeRecommendedGroupSize(1, 10);
        PublishedBridgesWithTwoKeyPoints.AddTransportTime(TransportMode.Car, 35);
        PublishedBridgesWithTwoKeyPoints.AddKeyPoint("Varadinski most", "Most koji spaja centar sa Petrovaradinom.", "/images/varadinski-most.jpg", new Location(45.2526, 19.8650), null, true);
        PublishedBridgesWithTwoKeyPoints.AddKeyPoint("Most slobode", "Viseći most na izlazu iz grada.", "/images/most-slobode.jpg", new Location(45.2390, 19.8290), "Most je obnovljen 2005. godine.", false);
        PublishedBridgesWithTwoKeyPoints.SetCalculatedLength(3.2);
        PublishedBridgesWithTwoKeyPoints.Publish();

        PublishedKalemegdanWithVisibleKeyPointInBelgrade = new(WellKnownUsers.SecondExplorer, "Kalemegdanska tvrđava", "Obilazak počinje kod spomenika Pobedniku na Kalemegdanu, vodi kroz Gornji i Donji grad i završava se na ušću Save u Dunav.", TourDifficulty.Easy, ["beograd"]);
        PublishedKalemegdanWithVisibleKeyPointInBelgrade.ChangeRecommendedGroupSize(1, 10);
        PublishedKalemegdanWithVisibleKeyPointInBelgrade.AddTransportTime(TransportMode.Walking, 60);
        PublishedKalemegdanWithVisibleKeyPointInBelgrade.ChangePrice(500);
        PublishedKalemegdanWithVisibleKeyPointInBelgrade.AddKeyPoint("Pobednik", "Spomenik Pobedniku iznad ušća.", "/images/pobednik.jpg", new Location(44.8231, 20.4504), null, true);
        PublishedKalemegdanWithVisibleKeyPointInBelgrade.AddKeyPoint("Ušće", "Mesto gde se Sava uliva u Dunav.", "/images/usce.jpg", new Location(44.8250, 20.4460), "Sa kule Nebojša vidi se cela reka.", false);
        PublishedKalemegdanWithVisibleKeyPointInBelgrade.SetCalculatedLength(1.4);
        PublishedKalemegdanWithVisibleKeyPointInBelgrade.Publish();

        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade = new(WellKnownUsers.SecondExplorer, "Skadarlija i Zemun", "Obilazak počinje u boemskoj Skadarliji, a nastavlja se preko reke do Gardoša i starog zemunskog keja, uz priču o dva grada koja su postala jedan.", TourDifficulty.Moderate, ["beograd"]);
        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.ChangeRecommendedGroupSize(2, 12);
        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.AddTransportTime(TransportMode.Bicycle, 90);
        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.ChangePrice(800);
        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.AddKeyPoint("Skadarlija", "Kaldrmisana ulica boema.", "/images/skadarlija.jpg", new Location(44.8150, 20.4600), "Najstarija kafana ima tajni podrum.", false);
        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.AddKeyPoint("Gardoš", "Kula na zemunskom brdu.", "/images/gardos.jpg", new Location(44.8433, 20.4011), null, true);
        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.SetCalculatedLength(7.9);
        PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Publish();

        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade = new(WellKnownUsers.Explorer, "Od Trga republike do Avale", "Tura počinje na Trgu republike, penje se do Avalskog tornja i vraća se u grad do Slavije, uz priču o tome kako je grad rastao ka jugu.", TourDifficulty.Hard, ["beograd"]);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.ChangeRecommendedGroupSize(2, 8);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.AddTransportTime(TransportMode.Car, 120);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.ChangePrice(2000);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.AddKeyPoint("Trg republike", "Glavni gradski trg.", "/images/trg-republike.jpg", new Location(44.8176, 20.4600), "Ispod trga je stari bunker.", false);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.AddKeyPoint("Avalski toranj", "Toranj na vrhu Avale.", "/images/avalski-toranj.jpg", new Location(44.6953, 20.5160), null, true);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.AddKeyPoint("Slavija", "Najveći kružni tok u gradu.", "/images/slavija.jpg", new Location(44.8027, 20.4663), null, true);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.SetCalculatedLength(29.6);
        PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Publish();

        DraftDorcolWithVisibleKeyPointInBelgrade = new(WellKnownUsers.Explorer, "Dorćol", "Kratak opis šetnje Dorćolom.", TourDifficulty.Easy, ["beograd"]);
        DraftDorcolWithVisibleKeyPointInBelgrade.AddKeyPoint("Bajrakli džamija", "Jedina sačuvana džamija u gradu.", "/images/bajrakli.jpg", new Location(44.8250, 20.4620), null, true);

        ArchivedTasmajdanWithVisibleKeyPointInBelgrade = new(WellKnownUsers.Explorer, "Tašmajdanski park", "Šetnja vodi kroz Tašmajdanski park, pored crkve Svetog Marka i starih rimskih kamenoloma, do zgrade Narodne skupštine na rubu parka.", TourDifficulty.Easy, ["beograd"]);
        ArchivedTasmajdanWithVisibleKeyPointInBelgrade.ChangeRecommendedGroupSize(1, 15);
        ArchivedTasmajdanWithVisibleKeyPointInBelgrade.AddTransportTime(TransportMode.Walking, 45);
        ArchivedTasmajdanWithVisibleKeyPointInBelgrade.AddKeyPoint("Crkva Svetog Marka", "Crkva na ivici parka.", "/images/sveti-marko.jpg", new Location(44.8090, 20.4730), null, true);
        ArchivedTasmajdanWithVisibleKeyPointInBelgrade.AddKeyPoint("Narodna skupština", "Zgrada skupštine na rubu parka.", "/images/skupstina.jpg", new Location(44.8117, 20.4655), "Kamenolomi ispod parka potiču iz rimskog doba.", false);
        ArchivedTasmajdanWithVisibleKeyPointInBelgrade.SetCalculatedLength(1.1);
        ArchivedTasmajdanWithVisibleKeyPointInBelgrade.Publish();
        ArchivedTasmajdanWithVisibleKeyPointInBelgrade.Archive();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, PublishedLakeside, PublishedFreeParkWalk, PublishedCheapGallery, PublishedModeratelyPricedFortress, PublishedExpensiveMountain, ArchivedCastle, DraftOldTownWithThreeKeyPoints, SecondExplorersDraftMarketWithTwoKeyPoints, PublishedBridgesWithTwoKeyPoints, PublishedKalemegdanWithVisibleKeyPointInBelgrade, PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade, PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade, DraftDorcolWithVisibleKeyPointInBelgrade, ArchivedTasmajdanWithVisibleKeyPointInBelgrade];
}
