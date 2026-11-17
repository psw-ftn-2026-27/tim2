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
    public async Task Player_in_range_activates_a_published_quiz_and_an_attempt_without_answers_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore + 1);
        var stored = assertContext.QuizAttempts.Single(attempt =>
            attempt.QuizId == QuizSeed.PublishedSynagogue.Id && attempt.PlayerId == WellKnownUsers.ThirdExplorer);
        stored.QuestionCount.Should().Be(QuizSeed.PublishedSynagogue.Questions.Count);
        stored.Answers.Should().BeEmpty();
        stored.SolvedAt.Should().BeNull();
    }

    [Fact]
    public async Task Activating_without_a_saved_position_is_rejected_and_no_attempt_is_created()
    {
        var playerWithoutPosition = Guid.NewGuid();
        var client = Factory.CreateClientFor(playerWithoutPosition, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.QuizAttempts.Should().NotContain(attempt => attempt.PlayerId == playerWithoutPosition);
    }

    [Fact]
    public async Task Activating_a_quiz_that_does_not_exist_is_rejected_and_no_attempt_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Activating_a_quiz_from_outside_its_range_is_rejected_and_no_attempt_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.QuizAttempts.Should().NotContain(attempt =>
            attempt.QuizId == QuizSeed.PublishedSynagogue.Id && attempt.PlayerId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Activating_a_quiz_that_is_not_published_is_rejected_even_in_range_and_no_attempt_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredSynagogueGarden.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.QuizAttempts.Should().NotContain(attempt =>
            attempt.QuizId == QuizSeed.RetiredSynagogueGarden.Id && attempt.PlayerId == WellKnownUsers.ThirdExplorer);
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
    public async Task Last_answer_solves_the_attempt_and_records_when_it_was_solved_and_the_earned_xp()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = QuizAttemptSeed.SecondExplorersSynagogueWithoutAnswers;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/answers", new AnswerQuizDto(0, 0), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.QuizAttempts.Single(attempt => attempt.Id == seeded.Id);
        stored.Answers.Should().ContainSingle().Which.IsCorrect.Should().BeTrue();
        stored.SolvedAt.Should().Be(stored.Answers[0].AnsweredAt);
        stored.EarnedXp.Should().Be(QuizSeed.PublishedSynagogue.Xp);
    }

    [Fact]
    public async Task Answering_a_quiz_the_player_has_not_activated_is_rejected_and_no_attempt_is_created()
    {
        var playerWithoutAttempt = Guid.NewGuid();
        var client = Factory.CreateClientFor(playerWithoutAttempt, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedDanubePark.Id}/answers", new AnswerQuizDto(0, 0), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.QuizAttempts.Should().NotContain(attempt => attempt.PlayerId == playerWithoutAttempt);
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
