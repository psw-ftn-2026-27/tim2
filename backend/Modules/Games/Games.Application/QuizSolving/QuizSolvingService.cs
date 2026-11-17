using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Positions.Contracts;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly QuizSolver _quizSolver;
    private readonly IPositionsApi _positionsApi;
    private readonly IUnitOfWork _unitOfWork;

    public QuizSolvingService(
        IQuizRepository quizRepository,
        IQuizAttemptRepository attemptRepository,
        QuizSolver quizSolver,
        IPositionsApi positionsApi,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _attemptRepository = attemptRepository;
        _quizSolver = quizSolver;
        _positionsApi = positionsApi;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid quizId, Guid playerId)
    {
        var position = await _positionsApi.GetPositionAsync(playerId);
        if (position is null)
        {
            throw new NotFoundException("Your position is not set. Click on the map to set it.");
        }
        var quiz = await GetQuizAsync(quizId);
        var existingAttempt = await _attemptRepository.GetByQuizAndPlayerAsync(quizId, playerId);

        var attempt = _quizSolver.Activate(quiz, existingAttempt, position.Latitude, position.Longitude, playerId, DateTime.UtcNow);

        _attemptRepository.Add(attempt);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AnswerAsync(Guid quizId, Guid playerId, AnswerQuizDto dto)
    {
        var quiz = await GetQuizAsync(quizId);
        var attempt = await _attemptRepository.GetByQuizAndPlayerAsync(quizId, playerId);
        if (attempt is null)
        {
            throw new NotFoundException($"You have not activated quiz {quizId}.");
        }

        _quizSolver.Answer(quiz, attempt, dto.QuestionIndex, dto.OptionIndex, DateTime.UtcNow);
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
}
