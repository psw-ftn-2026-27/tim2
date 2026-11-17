using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizManagement;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizManagement;

public class QuizManagementQueryTests : BaseIntegrationTest
{
    public QuizManagementQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Quiz_details_list_questions_and_answer_options_in_the_order_they_were_entered_with_the_correct_answer_marked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = QuizSeed.DraftMuseum;

        var quiz = await client.GetFromJsonAsync<QuizDto>($"/api/games/quizzes/{seeded.Id}", JsonOptions);

        quiz.Should().BeEquivalentTo(
            new QuizDto(
                seeded.Id,
                seeded.Name,
                seeded.Instructions,
                seeded.Xp,
                seeded.Status,
                seeded.Questions
                    .Select(question => new QuestionDto(
                        question.Text,
                        question.Options.Select(option => new AnswerOptionDto(option.Text, option.IsCorrect)).ToList()))
                    .ToList(),
                null),
            options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Quiz_details_show_its_location_and_range()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = QuizSeed.DraftCityHall;

        var quiz = await client.GetFromJsonAsync<QuizDto>($"/api/games/quizzes/{seeded.Id}", JsonOptions);

        quiz!.Location.Should().Be(new QuizLocationDto(seeded.Location!.Latitude, seeded.Location.Longitude, seeded.Location.RangeInMeters));
    }
}
