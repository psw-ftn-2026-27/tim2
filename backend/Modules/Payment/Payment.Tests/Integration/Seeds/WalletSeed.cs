using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersFundedWallet;

    static WalletSeed()
    {
        ExplorersFundedWallet = new(WellKnownUsers.Explorer);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 100);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 50);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 20);
    }

    public static Wallet[] All => [ExplorersFundedWallet];
}
