using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Positions.Domain.UserPositions;

namespace Positions.Infrastructure.Persistence.UserPositions;

internal static class UserPositionConfiguration
{
    public static void ConfigureUserPositions(this ModelBuilder modelBuilder)
    {
        ConfigureUserPosition(modelBuilder.Entity<UserPosition>());
    }

    private static void ConfigureUserPosition(EntityTypeBuilder<UserPosition> builder)
    {
        builder.HasIndex(position => position.UserId).IsUnique();

        builder.OwnsOne(position => position.Coordinates);
        builder.Navigation(position => position.Coordinates).IsRequired();
    }
}
