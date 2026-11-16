using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCarts;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCart?> GetByBuyerIdAsync(Guid buyerId)
    {
        return _dbContext.ShoppingCarts.FirstOrDefaultAsync(cart => cart.BuyerId == buyerId);
    }

    public void Add(ShoppingCart cart)
    {
        _dbContext.ShoppingCarts.Add(cart);
    }
}
