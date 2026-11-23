using Exploration.Domain.TourLocationSearchRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TourLocationSearchRecords;

internal static class TourLocationSearchRecordConfiguration
{
    public static void ConfigureTourLocationSearchRecords(this ModelBuilder modelBuilder)
    {
        ConfigureTourLocationSearchRecord(modelBuilder.Entity<TourLocationSearchRecord>());
    }

    private static void ConfigureTourLocationSearchRecord(EntityTypeBuilder<TourLocationSearchRecord> builder)
    {
        builder.OwnsOne(record => record.Area, area =>
        {
            area.OwnsOne(a => a.Center);
            area.Navigation(a => a.Center).IsRequired();
        });
        builder.Navigation(record => record.Area).IsRequired();

        builder.PrimitiveCollection(record => record.Difficulties).ElementType().HasConversion<string>();
        builder.PrimitiveCollection(record => record.TransportModes).ElementType().HasConversion<string>();
    }
}
