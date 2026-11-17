using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Shopping;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/shopping")]
[Authorize(Roles = "explorer")]
public sealed class ShoppingController : ControllerBase
{
    private readonly ShoppingService _shoppingService;
    private readonly ShoppingQueries _shoppingQueries;

    public ShoppingController(ShoppingService shoppingService, ShoppingQueries shoppingQueries)
    {
        _shoppingService = shoppingService;
        _shoppingQueries = shoppingQueries;
    }

    [HttpGet("tours/{tourId:guid}")]
    public async Task<ActionResult<TourOfferDto>> GetTourOffer(Guid tourId)
    {
        return await _shoppingQueries.GetTourOfferAsync(tourId);
    }

    [HttpGet("cart")]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        return await _shoppingQueries.GetCartAsync(User.GetUserId());
    }

    [HttpPost("cart/items/{tourId:guid}")]
    public async Task<ActionResult> AddToCart(Guid tourId)
    {
        await _shoppingService.AddToCartAsync(User.GetUserId(), tourId);
        return NoContent();
    }

    [HttpDelete("cart/items/{tourId:guid}")]
    public async Task<ActionResult> RemoveFromCart(Guid tourId)
    {
        await _shoppingService.RemoveFromCartAsync(User.GetUserId(), tourId);
        return NoContent();
    }

    [HttpPost("cart/purchase")]
    public async Task<ActionResult> Purchase()
    {
        await _shoppingService.PurchaseAsync(User.GetUserId());
        return NoContent();
    }
}
