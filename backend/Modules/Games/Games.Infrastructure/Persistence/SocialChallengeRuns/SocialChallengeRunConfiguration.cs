using Games.Domain.SocialChallengeRuns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.SocialChallengeRuns;

internal static class SocialChallengeRunConfiguration
{
    public static void ConfigureSocialChallengeRuns(this ModelBuilder modelBuilder)
    {
        ConfigureSocialChallengeRun(modelBuilder.Entity<SocialChallengeRun>());
        ConfigureSocialChallengeParticipant(modelBuilder.Entity<SocialChallengeParticipant>());
    }

    private static void ConfigureSocialChallengeRun(EntityTypeBuilder<SocialChallengeRun> builder)
    {
        builder.Property(run => run.Status).HasConversion<string>().HasMaxLength(20);

        // Each participant is a row of its own, so two players checking at the same time do not overwrite each other.
        builder.HasMany(run => run.Participants)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(run => run.Participants).AutoInclude();
    }

    private static void ConfigureSocialChallengeParticipant(EntityTypeBuilder<SocialChallengeParticipant> builder)
    {
        builder.ToTable("SocialChallengeParticipants");
        builder.Property(participant => participant.Id).ValueGeneratedNever();

        // Join rejects a player who is already in the run, but two activations at the same moment both pass that
        // check; the database then refuses the second row. The run's key is a shadow property, so it is named here.
        builder.HasIndex("SocialChallengeRunId", nameof(SocialChallengeParticipant.PlayerId)).IsUnique();
    }
}
