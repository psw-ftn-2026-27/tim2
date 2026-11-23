using Payment.Domain.ShoppingCarts;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByBuyerIdAsync(Guid buyerId);

    void Add(ShoppingCart cart);
}
