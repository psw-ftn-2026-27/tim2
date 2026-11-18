using Payment.Domain.GiftCards;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class GiftCardSeed
{
    public static readonly GiftCard ExplorerToSecondExplorer;
    public static readonly GiftCard SecondExplorerToExplorer;
    public static readonly GiftCard SecondExplorerToThirdExplorer;

    static GiftCardSeed()
    {
        ExplorerToSecondExplorer = new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 50, "Za tvoju sledecu turu", DateTime.UtcNow);
        SecondExplorerToExplorer = new(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer, 20, "Hvala za savet", DateTime.UtcNow);
        SecondExplorerToThirdExplorer = new(WellKnownUsers.SecondExplorer, WellKnownUsers.ThirdExplorer, 30, "Srecan rodjendan", DateTime.UtcNow);
    }

    public static GiftCard[] All => [ExplorerToSecondExplorer, SecondExplorerToExplorer, SecondExplorerToThirdExplorer];
}
