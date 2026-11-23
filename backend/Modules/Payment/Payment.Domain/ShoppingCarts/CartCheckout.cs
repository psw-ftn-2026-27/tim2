using Payment.Domain.TourPurchaseTokens;
using Payment.Domain.Wallets;

namespace Payment.Domain.ShoppingCarts;

public sealed class CartCheckout
{
    public List<TourPurchaseToken> Checkout(ShoppingCart cart, Wallet wallet, IReadOnlyCollection<Guid> publishedTourIds)
    {
        var items = cart.Checkout(publishedTourIds);
        wallet.Purchase(items);

        return items
            .Select(item => new TourPurchaseToken(cart.BuyerId, item.EntityId, item.Name, item.Price))
            .ToList();
    }
}
