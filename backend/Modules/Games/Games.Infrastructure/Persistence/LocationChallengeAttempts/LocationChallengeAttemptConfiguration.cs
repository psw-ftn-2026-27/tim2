using Games.Domain.LocationChallengeAttempts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal static class LocationChallengeAttemptConfiguration
{
    public static void ConfigureLocationChallengeAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallengeAttempt(modelBuilder.Entity<LocationChallengeAttempt>());
    }

    private static void ConfigureLocationChallengeAttempt(EntityTypeBuilder<LocationChallengeAttempt> builder)
    {
        // A player has one attempt per round of a challenge, even when two activations arrive at the same time.
        builder.HasIndex(attempt => new { attempt.ChallengeId, attempt.PlayerId, attempt.Round }).IsUnique();

        // Attempts stored before discoveries existed get an empty list instead of failing the migration.
        builder.Property(attempt => attempt.DiscoveredPointNumbers).HasDefaultValueSql("'{}'");
    }
}
