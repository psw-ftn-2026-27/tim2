using Payment.Application.Shopping;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartReadRepository
{
    Task<CartDto?> GetByBuyerIdAsync(Guid buyerId);
}
