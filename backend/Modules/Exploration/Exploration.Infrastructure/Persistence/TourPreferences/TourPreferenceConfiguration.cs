using Exploration.Domain.TourPreferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal static class TourPreferenceConfiguration
{
    public static void ConfigureTourPreferences(this ModelBuilder modelBuilder)
    {
        ConfigureTourPreference(modelBuilder.Entity<TourPreference>());
    }

    private static void ConfigureTourPreference(EntityTypeBuilder<TourPreference> builder)
    {
        builder.HasIndex(preference => preference.TouristId).IsUnique();

        builder.OwnsOne(preference => preference.Criteria, criteria =>
        {
            criteria.PrimitiveCollection(c => c.Difficulties).ElementType().HasConversion<string>();
            criteria.PrimitiveCollection(c => c.TransportModes).ElementType().HasConversion<string>();
        });
        builder.Navigation(preference => preference.Criteria).IsRequired();
    }
}
