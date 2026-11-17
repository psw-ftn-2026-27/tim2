using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.TourPurchaseTokens;

namespace Payment.Infrastructure.Persistence.TourPurchaseTokens;

internal static class TourPurchaseTokenConfiguration
{
    public static void ConfigureTourPurchaseTokens(this ModelBuilder modelBuilder)
    {
        ConfigureTourPurchaseToken(modelBuilder.Entity<TourPurchaseToken>());
    }

    private static void ConfigureTourPurchaseToken(EntityTypeBuilder<TourPurchaseToken> builder)
    {
        builder.HasIndex(token => token.BuyerId);
        builder.Property(token => token.Name).HasMaxLength(200);
    }
}
