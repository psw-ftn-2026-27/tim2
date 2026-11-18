using Exploration.Domain.TourSessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TourSessions;

internal static class TourSessionConfiguration
{
    public static void ConfigureTourSessions(this ModelBuilder modelBuilder)
    {
        ConfigureTourSession(modelBuilder.Entity<TourSession>());
    }

    private static void ConfigureTourSession(EntityTypeBuilder<TourSession> builder)
    {
        builder.Property(session => session.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(session => session.StartPosition);
        builder.Navigation(session => session.StartPosition).IsRequired();
    }
}
