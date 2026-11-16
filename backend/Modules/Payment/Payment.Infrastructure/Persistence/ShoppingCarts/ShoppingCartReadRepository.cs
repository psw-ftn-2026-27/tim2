using Microsoft.EntityFrameworkCore;
using Payment.Application.Shopping;
using Payment.Application.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartReadRepository : IShoppingCartReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CartDto?> GetByBuyerIdAsync(Guid buyerId)
    {
        return _dbContext.ShoppingCarts
            .Where(cart => cart.BuyerId == buyerId)
            .Select(cart => new CartDto(
                cart.Items
                    .Select(item => new CartItemDto(item.EntityId, item.Name, item.Price))
                    .ToList(),
                cart.Items.Sum(item => item.Price)))
            .FirstOrDefaultAsync();
    }
}
