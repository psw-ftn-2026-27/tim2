using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.CartPurchasing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/shopping")]
[Authorize(Roles = "explorer")]
public sealed class CartPurchasingController : ControllerBase
{
    private readonly CartPurchasingService _cartPurchasingService;

    public CartPurchasingController(CartPurchasingService cartPurchasingService)
    {
        _cartPurchasingService = cartPurchasingService;
    }

    [HttpPost("cart/purchase")]
    public async Task<ActionResult> Purchase()
    {
        await _cartPurchasingService.PurchaseAsync(User.GetUserId());
        return NoContent();
    }
}
