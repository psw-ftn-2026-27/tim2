using Games.Application.Quizzes;
using Games.Domain.Locations;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizManagement;

public sealed class QuizManagementService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizManagementService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateQuizDto dto)
    {
        var quiz = new Quiz(dto.Name, dto.Instructions, dto.Xp, ToQuestions(dto.Questions), ToLocation(dto.Location));

        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();

        return quiz.Id;
    }

    public async Task UpdateAsync(Guid quizId, UpdateQuizDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.Update(dto.Name, dto.Instructions, dto.Xp, ToQuestions(dto.Questions), ToLocation(dto.Location));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid quizId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.EnsureCanBeDeleted();
        _quizRepository.Remove(quiz);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        return quiz;
    }

    private static List<Question> ToQuestions(List<QuestionDto>? questions)
    {
        return (questions ?? [])
            .Select(question => new Question(
                question.Text,
                (question.Options ?? [])
                    .Select(option => new AnswerOption(option.Text, option.IsCorrect))
                    .ToList()))
            .ToList();
    }

    private static QuizLocation? ToLocation(QuizLocationDto? location)
    {
        return location is null
            ? null
            : new QuizLocation(new Location(location.Latitude, location.Longitude), location.RangeInMeters);
    }
}
