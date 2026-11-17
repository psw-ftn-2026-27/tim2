using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizSolving;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingQueryTests : BaseIntegrationTest
{
    public QuizSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Current_question_lists_the_answer_options_without_revealing_the_correct_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var question = QuizSeed.PublishedDanubePark.Questions[1];

        var response = await client.GetAsync($"/api/games/quizzes/{QuizSeed.PublishedDanubePark.Id}/progress");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContainEquivalentOf("isCorrect");
        var progress = await response.Content.ReadFromJsonAsync<QuizProgressDto>(JsonOptions);
        progress!.CurrentQuestion.Should().BeEquivalentTo(
            new CurrentQuestionDto(1, question.Text, question.Options.Select(option => option.Text).ToList()),
            options => options.ComparingRecordsByMembers().WithStrictOrdering());
        progress.QuestionCount.Should().Be(QuizSeed.PublishedDanubePark.Questions.Count);
        progress.CorrectCount.Should().Be(1);
        progress.LastAnswerCorrect.Should().BeTrue();
        progress.EarnedXp.Should().BeNull();
    }

    [Fact]
    public async Task Solved_attempt_shows_the_recorded_xp_even_after_the_quiz_xp_changed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = QuizAttemptSeed.ExplorersFishermansIslandSolvedWithOneOfTwo;

        var progress = await client.GetFromJsonAsync<QuizProgressDto>(
            $"/api/games/quizzes/{QuizSeed.RepeatedFishermansIslandWithMoreXp.Id}/progress", JsonOptions);

        progress!.EarnedXp.Should().Be(seeded.EarnedXp);
        progress.CorrectCount.Should().Be(1);
        progress.Percentage.Should().Be(50);
        progress.SolvedAt.Should().Be(seeded.SolvedAt);
        progress.CurrentQuestion.Should().BeNull();
    }
}
