using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal static class LocationChallengeConfiguration
{
    public static void ConfigureLocationChallenges(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallenge(modelBuilder.Entity<LocationChallenge>());
    }

    private static void ConfigureLocationChallenge(EntityTypeBuilder<LocationChallenge> builder)
    {
        builder.Property(challenge => challenge.Name).HasMaxLength(60);
        builder.Property(challenge => challenge.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(challenge => challenge.StartLocation);
        builder.Navigation(challenge => challenge.StartLocation).IsRequired();

        builder.OwnsMany(challenge => challenge.HiddenPoints, hiddenPoints =>
        {
            hiddenPoints.ToJson();
            hiddenPoints.OwnsOne(point => point.Location);
        });
    }
}
