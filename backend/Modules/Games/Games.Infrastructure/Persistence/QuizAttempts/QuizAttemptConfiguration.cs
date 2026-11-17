using Games.Domain.QuizAttempts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal static class QuizAttemptConfiguration
{
    public static void ConfigureQuizAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureQuizAttempt(modelBuilder.Entity<QuizAttempt>());
    }

    private static void ConfigureQuizAttempt(EntityTypeBuilder<QuizAttempt> builder)
    {
        // A player has one attempt per quiz, even when two activations arrive at the same time.
        builder.HasIndex(attempt => new { attempt.QuizId, attempt.PlayerId }).IsUnique();

        builder.OwnsMany(attempt => attempt.Answers, answers => answers.ToJson());
    }
}
