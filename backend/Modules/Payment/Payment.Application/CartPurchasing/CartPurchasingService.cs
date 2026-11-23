using Exploration.Contracts;
using Payment.Application.ShoppingCarts;
using Payment.Application.TourPurchaseTokens;
using Payment.Application.Wallets;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.CartPurchasing;

public sealed class CartPurchasingService
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITourPurchaseTokenRepository _tokenRepository;
    private readonly CartCheckout _cartCheckout;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExplorationApi _explorationApi;

    public CartPurchasingService(
        IShoppingCartRepository cartRepository,
        IWalletRepository walletRepository,
        ITourPurchaseTokenRepository tokenRepository,
        CartCheckout cartCheckout,
        IUnitOfWork unitOfWork,
        IExplorationApi explorationApi)
    {
        _cartRepository = cartRepository;
        _walletRepository = walletRepository;
        _tokenRepository = tokenRepository;
        _cartCheckout = cartCheckout;
        _unitOfWork = unitOfWork;
        _explorationApi = explorationApi;
    }

    public async Task PurchaseAsync(Guid buyerId)
    {
        var cart = await _cartRepository.GetByBuyerIdAsync(buyerId);
        if (cart is null)
        {
            throw new NotFoundException($"Cart of buyer {buyerId} does not exist.");
        }

        var publishedTourIds = await GetPublishedTourIdsAsync(cart);
        var wallet = await _walletRepository.GetByUserIdAsync(buyerId);
        if (wallet is null)
        {
            wallet = new Wallet(buyerId);
            _walletRepository.Add(wallet);
        }

        var tokens = _cartCheckout.Checkout(cart, wallet, publishedTourIds);
        foreach (var token in tokens)
        {
            _tokenRepository.Add(token);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<List<Guid>> GetPublishedTourIdsAsync(ShoppingCart cart)
    {
        var publishedTourIds = new List<Guid>();
        foreach (var item in cart.Items)
        {
            var tour = await _explorationApi.GetPublishedTourAsync(item.EntityId);
            if (tour is not null)
            {
                publishedTourIds.Add(tour.TourId);
            }
        }
        return publishedTourIds;
    }
}
