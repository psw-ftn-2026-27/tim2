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

        builder.OwnsMany(session => session.CompletedKeyPoints, completions => completions.ToJson());

        // A tourist has at most one active session (P3). Two starts at the same moment both find no active session;
        // the database then refuses the second one, so the tourist never ends up with two active tours.
        // The status is stored as a string, so the filter compares it with the text 'Active'.
        builder.HasIndex(session => session.TouristId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");
    }
}
