using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersFundedWallet;
    public static readonly Wallet SecondExplorersFundedWallet;

    static WalletSeed()
    {
        ExplorersFundedWallet = new(WellKnownUsers.Explorer);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 100);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 50);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 20);

        SecondExplorersFundedWallet = new(WellKnownUsers.SecondExplorer);
        SecondExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 300);
    }

    public static Wallet[] All => [ExplorersFundedWallet, SecondExplorersFundedWallet];
}
