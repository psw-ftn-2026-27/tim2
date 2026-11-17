using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizAttempts;

public sealed class QuizSolver
{
    public QuizAttempt Activate(Quiz quiz, QuizAttempt? existingAttempt, double latitude, double longitude, Guid playerId, DateTime now)
    {
        if (quiz.Status != QuizStatus.Published)
        {
            throw new DomainException("Only a published quiz can be activated.");
        }
        if (existingAttempt is not null)
        {
            throw new DomainException("You have already activated this quiz, and a quiz has only one attempt.");
        }
        if (!quiz.IsWithinRange(latitude, longitude))
        {
            throw new DomainException("You are outside the quiz range. Come closer to activate it.");
        }

        return new QuizAttempt(quiz.Id, playerId, quiz.Questions.Count, now);
    }

    // Neither the range nor the quiz state is checked here: an activated quiz can be finished from anywhere,
    // even after the quiz is retired.
    public void Answer(Quiz quiz, QuizAttempt attempt, int expectedQuestionIndex, int optionIndex, DateTime now)
    {
        var questionIndex = attempt.NextQuestionIndex();
        if (expectedQuestionIndex != questionIndex)
        {
            throw new DomainException("This question is not the current question of the quiz.");
        }
        if (quiz.Questions.Count != attempt.QuestionCount)
        {
            throw new DomainException("The quiz questions changed after you activated it, so this attempt cannot be finished.");
        }

        var isCorrect = quiz.IsCorrect(questionIndex, optionIndex);
        attempt.RecordAnswer(new QuizAnswer(optionIndex, isCorrect, now), quiz.Xp);
    }
}
