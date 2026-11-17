using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.CartPurchasing;
using Payment.Application.Products;
using Payment.Application.PurchaseViewing;
using Payment.Application.Shopping;
using Payment.Application.ShoppingCarts;
using Payment.Application.TourPurchaseTokens;
using Payment.Application.WalletFunding;
using Payment.Application.Wallets;
using Payment.Application.WalletViewing;
using Payment.Domain.ShoppingCarts;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Payment.Infrastructure.Persistence.TourPurchaseTokens;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Infrastructure;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IShoppingCartReadRepository, ShoppingCartReadRepository>();
        services.AddScoped<ITourPurchaseTokenRepository, TourPurchaseTokenRepository>();
        services.AddScoped<ITourPurchaseTokenReadRepository, TourPurchaseTokenReadRepository>();

        services.AddScoped<CartCheckout>();

        services.AddScoped<WalletViewingQueries>();
        services.AddScoped<WalletFundingService>();
        services.AddScoped<WalletFundingQueries>();
        services.AddScoped<ShoppingService>();
        services.AddScoped<ShoppingQueries>();
        services.AddScoped<CartPurchasingService>();
        services.AddScoped<PurchaseViewingQueries>();

        return services;
    }
}
