using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.Quizzes;

internal static class QuizConfiguration
{
    public static void ConfigureQuizzes(this ModelBuilder modelBuilder)
    {
        ConfigureQuiz(modelBuilder.Entity<Quiz>());
    }

    private static void ConfigureQuiz(EntityTypeBuilder<Quiz> builder)
    {
        builder.Property(quiz => quiz.Name).HasMaxLength(60);
        builder.Property(quiz => quiz.Instructions).HasMaxLength(1000);
        builder.Property(quiz => quiz.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(quiz => quiz.Questions, questions =>
        {
            questions.ToJson();
            questions.OwnsMany(question => question.Options);
        });

        builder.OwnsOne(quiz => quiz.Location, location =>
        {
            location.OwnsOne(quizLocation => quizLocation.Location);
            location.Navigation(quizLocation => quizLocation.Location).IsRequired();
        });
    }
}
