using System.Net;
using FluentAssertions;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizLifecycle;

public class QuizLifecycleCommandTests : BaseIntegrationTest
{
    public QuizLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_quiz_and_it_is_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftCityHall.Id);
        stored.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Publishing_an_unknown_quiz_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{unknownId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == unknownId);
    }

    [Fact]
    public async Task Publishing_an_already_published_quiz_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.PublishedSynagogue.Id);
        stored.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_quiz_and_it_is_stored_as_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.PublishedSynagogue.Id);
        stored.Status.Should().Be(QuizStatus.Retired);
    }

    [Fact]
    public async Task Retiring_an_unknown_quiz_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{unknownId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == unknownId);
    }

    [Fact]
    public async Task Retiring_a_draft_quiz_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftCityHall.Id);
        stored.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_quiz_and_it_is_stored_as_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredBishopsPalace.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.RetiredBishopsPalace.Id);
        stored.Status.Should().Be(QuizStatus.Archived);
    }

    [Fact]
    public async Task Archiving_an_unknown_quiz_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{unknownId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == unknownId);
    }

    [Fact]
    public async Task Archiving_a_draft_quiz_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftCityHall.Id);
        stored.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredBishopsPalace.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredBishopsPalace.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_repeats_an_archived_quiz_and_it_is_stored_as_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.ArchivedZmajJovinaStreet.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.ArchivedZmajJovinaStreet.Id);
        stored.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public async Task Repeating_an_unknown_quiz_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsync($"/api/games/quizzes/{unknownId}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == unknownId);
    }

    [Fact]
    public async Task Repeating_a_published_quiz_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.PublishedSynagogue.Id);
        stored.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_repeat_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.ArchivedZmajJovinaStreet.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_repeat_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.ArchivedZmajJovinaStreet.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
