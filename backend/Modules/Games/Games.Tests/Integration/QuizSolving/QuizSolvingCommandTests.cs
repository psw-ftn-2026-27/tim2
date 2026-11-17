using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizSolving;
using Games.Domain.QuizAttempts;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingCommandTests : BaseIntegrationTest
{
    public QuizSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Activating_without_a_saved_position_is_rejected_and_no_attempt_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.QuizAttempts.Should().NotContain(attempt =>
            attempt.QuizId == QuizSeed.PublishedSynagogue.Id && attempt.PlayerId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Player_answers_the_current_question_and_the_answer_is_recorded_in_the_attempt()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = QuizAttemptSeed.ExplorersDanubeParkAfterFirstAnswer;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedDanubePark.Id}/answers", new AnswerQuizDto(1, 0), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.QuizAttempts.Single(attempt => attempt.Id == seeded.Id);
        stored.Answers.Should().HaveCount(2);
        stored.Answers[0].Should().Be(seeded.Answers[0]);
        stored.Answers[1].SelectedOptionIndex.Should().Be(0);
        stored.Answers[1].IsCorrect.Should().BeTrue();
        stored.SolvedAt.Should().BeNull();
    }

    [Fact]
    public async Task Answering_a_quiz_the_player_has_not_activated_is_rejected_and_no_attempt_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedDanubePark.Id}/answers", new AnswerQuizDto(0, 0), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.QuizAttempts.Should().NotContain(attempt => attempt.PlayerId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Answer_for_a_question_other_than_the_current_one_is_rejected_and_nothing_is_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = QuizAttemptSeed.ExplorersDanubeParkAfterFirstAnswer;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedDanubePark.Id}/answers", new AnswerQuizDto(2, 0), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.QuizAttempts.Single(attempt => attempt.Id == seeded.Id);
        stored.Answers.Should().Equal(seeded.Answers);
    }

    [Fact]
    public async Task Anonymous_user_cannot_answer_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedDanubePark.Id}/answers", new AnswerQuizDto(1, 0), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_answer_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedDanubePark.Id}/answers", new AnswerQuizDto(1, 0), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
