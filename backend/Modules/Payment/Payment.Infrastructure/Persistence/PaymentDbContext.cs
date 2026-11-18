using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.TourPurchaseTokens;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.GiftCards;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Payment.Infrastructure.Persistence.TourPurchaseTokens;
using Payment.Infrastructure.Persistence.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<TourPurchaseToken> TourPurchaseTokens => Set<TourPurchaseToken>();

    public DbSet<GiftCard> GiftCards => Set<GiftCard>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureProducts();
        builder.ConfigureShoppingCarts();
        builder.ConfigureTourPurchaseTokens();
        builder.ConfigureGiftCards();
    }
}
