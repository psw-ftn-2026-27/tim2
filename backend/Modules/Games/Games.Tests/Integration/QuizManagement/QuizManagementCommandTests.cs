using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizManagement;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizManagement;

public class QuizManagementCommandTests : BaseIntegrationTest
{
    public QuizManagementCommandTests(GamesApiFactory factory) : base(factory) { }

    private static CreateQuizDto CreateRequest() => new(
        "Katedrala Imena Marijinog",
        "Uđite u katedralu i pogledajte vitraže.",
        40,
        [
            new QuestionDto("Koliko je visok toranj katedrale?", [new AnswerOptionDto("72 metra", true), new AnswerOptionDto("30 metara", false)]),
            new QuestionDto("Kojim stilom je građena?", [new AnswerOptionDto("Barok", false), new AnswerOptionDto("Neogotika", true), new AnswerOptionDto("Secesija", false)])
        ]);

    private static UpdateQuizDto UpdateRequest() => new(
        "Gradska kuća na Trgu slobode",
        "Obiđite Gradsku kuću i pogledajte salu za venčanja.",
        60,
        [
            new QuestionDto("Koje godine je završena Gradska kuća?", [new AnswerOptionDto("1895", true), new AnswerOptionDto("1920", false)])
        ]);

    private static List<QuestionDto> ToDtos(IEnumerable<Question> questions) =>
        questions
            .Select(question => new QuestionDto(
                question.Text,
                question.Options.Select(option => new AnswerOptionDto(option.Text, option.IsCorrect)).ToList()))
            .ToList();

    [Fact]
    public async Task Administrator_creates_a_draft_quiz_with_its_questions_and_correct_answers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateRequest();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore + 1);
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == createdId);
        stored.Status.Should().Be(QuizStatus.Draft);
        stored.Name.Should().Be(request.Name);
        stored.Instructions.Should().Be(request.Instructions);
        stored.Xp.Should().Be(request.Xp);
        ToDtos(stored.Questions).Should().BeEquivalentTo(request.Questions, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Quiz_without_questions_is_rejected_and_no_quiz_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateRequest() with { Questions = [] };
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", CreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/games/quizzes", CreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_a_draft_quiz_and_only_the_new_questions_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = UpdateRequest();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftCityHall.Id);
        stored.Name.Should().Be(request.Name);
        stored.Instructions.Should().Be(request.Instructions);
        stored.Xp.Should().Be(request.Xp);
        stored.Status.Should().Be(QuizStatus.Draft);
        ToDtos(stored.Questions).Should().BeEquivalentTo(request.Questions, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Changing_an_unknown_quiz_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{unknownId}", UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == unknownId);
    }

    [Fact]
    public async Task Changing_a_published_quiz_is_rejected_and_its_stored_content_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = QuizSeed.PublishedSynagogue;

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{seeded.Id}", UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == seeded.Id);
        stored.Name.Should().Be(seeded.Name);
        stored.Instructions.Should().Be(seeded.Instructions);
        stored.Xp.Should().Be(seeded.Xp);
        stored.Status.Should().Be(QuizStatus.Published);
        ToDtos(stored.Questions).Should().BeEquivalentTo(ToDtos(seeded.Questions), options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}", UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftCityHall.Id}", UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_a_draft_quiz_and_it_is_gone()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressArchitecture.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore - 1);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == QuizSeed.DraftFortressArchitecture.Id);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_quiz_and_it_is_gone()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.ArchivedZmajJovinaStreet.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore - 1);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == QuizSeed.ArchivedZmajJovinaStreet.Id);
    }

    [Fact]
    public async Task Deleting_a_published_quiz_is_rejected_and_the_quiz_stays_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.PublishedSynagogue.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().Contain(quiz => quiz.Id == QuizSeed.PublishedSynagogue.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressArchitecture.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressArchitecture.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
