using Exploration.Tests.Integration.Seeds;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Guid WealthyBuyer = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    public static readonly Guid BuyerOfArchivedTour = Guid.Parse("00000000-0000-0000-0000-0000000000a2");

    public static readonly Wallet ExplorersFundedWallet;
    public static readonly Wallet WealthyBuyersWalletWithKalemegdanAndSkadarlijaPurchases;
    public static readonly Wallet BuyerOfArchivedToursWalletWithAvalaPurchase;

    static WalletSeed()
    {
        ExplorersFundedWallet = new(WellKnownUsers.Explorer);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 100);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 50);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 20);
        ExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 30);
        ExplorersFundedWallet.CancelDeposit(ExplorersFundedWallet.Transactions[3].Id, WellKnownUsers.Administrator, DateTime.UtcNow);

        WealthyBuyersWalletWithKalemegdanAndSkadarlijaPurchases = new(WealthyBuyer);
        WealthyBuyersWalletWithKalemegdanAndSkadarlijaPurchases.Deposit(WellKnownUsers.Administrator, 5000);
        WealthyBuyersWalletWithKalemegdanAndSkadarlijaPurchases.Purchase([
            new OrderItem(TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Name, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Price),
            new OrderItem(TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Id, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Name, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Price)]);

        BuyerOfArchivedToursWalletWithAvalaPurchase = new(BuyerOfArchivedTour);
        BuyerOfArchivedToursWalletWithAvalaPurchase.Deposit(WellKnownUsers.Administrator, 5000);
        BuyerOfArchivedToursWalletWithAvalaPurchase.Purchase([
            new OrderItem(TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Id, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Name, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Price)]);
    }

    public static Wallet[] All => [ExplorersFundedWallet, WealthyBuyersWalletWithKalemegdanAndSkadarlijaPurchases, BuyerOfArchivedToursWalletWithAvalaPurchase];
}
