using Exploration.Contracts;
using Payment.Application.Products;
using Payment.Application.ShoppingCarts;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.Shopping;

public sealed class ShoppingService
{
    private readonly IProductRepository _productRepository;
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExplorationApi _explorationApi;

    public ShoppingService(IProductRepository productRepository, IShoppingCartRepository cartRepository, IUnitOfWork unitOfWork, IExplorationApi explorationApi)
    {
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _explorationApi = explorationApi;
    }

    public async Task AddToCartAsync(Guid buyerId, Guid tourId)
    {
        var tour = await _explorationApi.GetPublishedTourAsync(tourId);
        if (tour is null)
        {
            throw new NotFoundException($"Published tour {tourId} does not exist.");
        }

        var product = await _productRepository.GetByEntityIdAsync(tourId);
        if (product is null)
        {
            product = new Product(tour.TourId, tour.Name, tour.AuthorId, tour.Price);
            _productRepository.Add(product);
        }

        var cart = await _cartRepository.GetByBuyerIdAsync(buyerId);
        if (cart is null)
        {
            cart = new ShoppingCart(buyerId);
            _cartRepository.Add(cart);
        }

        cart.AddItem(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(Guid buyerId, Guid tourId)
    {
        var cart = await _cartRepository.GetByBuyerIdAsync(buyerId);
        if (cart is null)
        {
            throw new NotFoundException($"Cart of buyer {buyerId} does not exist.");
        }

        cart.RemoveItem(tourId);
        await _unitOfWork.SaveChangesAsync();
    }
}
