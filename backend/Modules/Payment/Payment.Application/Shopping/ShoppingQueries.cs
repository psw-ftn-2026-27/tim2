using Exploration.Contracts;
using Payment.Application.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.Shopping;

public sealed class ShoppingQueries
{
    private readonly IShoppingCartReadRepository _cartReadRepository;
    private readonly IExplorationApi _explorationApi;

    public ShoppingQueries(IShoppingCartReadRepository cartReadRepository, IExplorationApi explorationApi)
    {
        _cartReadRepository = cartReadRepository;
        _explorationApi = explorationApi;
    }

    public async Task<TourOfferDto> GetTourOfferAsync(Guid tourId)
    {
        var tour = await _explorationApi.GetPublishedTourAsync(tourId);
        if (tour is null)
        {
            throw new NotFoundException($"Published tour {tourId} does not exist.");
        }

        return new TourOfferDto(tour.TourId, tour.Name, tour.Price);
    }

    public async Task<CartDto> GetCartAsync(Guid buyerId)
    {
        var cart = await _cartReadRepository.GetByBuyerIdAsync(buyerId);

        return cart ?? new CartDto([], 0);
    }
}
