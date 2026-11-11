using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club HikingClub = new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);
    public static readonly Club FortressGuides = new(WellKnownUsers.Explorer, "Vodiči kroz tvrđavu", "Zajednički obilasci podzemnih galerija Petrovaradinske tvrđave.", ["https://example.com/galerije.jpg", "https://example.com/sahat-kula.jpg"]);
    public static readonly Club SecondExplorersCyclingClub = new(WellKnownUsers.SecondExplorer, "Biciklisti uz Dunav", "Vožnje biciklom duž dunavskog keja.", ["https://example.com/biciklisti.jpg"]);

    public static Club[] All => [HikingClub, FortressGuides, SecondExplorersCyclingClub];
}
