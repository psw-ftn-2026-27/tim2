using Exploration.Application.TourSessions;
using Exploration.Domain.Locations;
using Exploration.Domain.TourSessions;
using Payment.Contracts;
using Positions.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecution;

public sealed class TourExecutionService
{
    private readonly ITourSessionRepository _sessionRepository;
    private readonly TourStarting _tourStarting;
    private readonly IPaymentApi _paymentApi;
    private readonly IPositionsApi _positionsApi;
    private readonly IUnitOfWork _unitOfWork;

    public TourExecutionService(
        ITourSessionRepository sessionRepository,
        TourStarting tourStarting,
        IPaymentApi paymentApi,
        IPositionsApi positionsApi,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _tourStarting = tourStarting;
        _paymentApi = paymentApi;
        _positionsApi = positionsApi;
        _unitOfWork = unitOfWork;
    }

    public async Task StartAsync(Guid touristId, Guid tourId)
    {
        var isPurchased = await _paymentApi.HasPurchasedAsync(touristId, tourId);
        var position = await _positionsApi.GetPositionAsync(touristId);
        var location = position is null ? null : new Location(position.Latitude, position.Longitude);
        var activeSession = await _sessionRepository.GetActiveByTouristAsync(touristId);

        var session = _tourStarting.Start(touristId, tourId, isPurchased, location, activeSession);

        // A resumed session is already stored, so only a new one is added.
        if (session != activeSession)
        {
            _sessionRepository.Add(session);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AbandonAsync(Guid touristId, Guid tourId)
    {
        var session = await _sessionRepository.GetActiveByTouristAndTourAsync(touristId, tourId);
        if (session is null)
        {
            throw new NotFoundException($"Tour {tourId} is not active for you.");
        }

        session.Abandon();
        await _unitOfWork.SaveChangesAsync();
    }
}
