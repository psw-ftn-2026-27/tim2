using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.PurchaseViewing;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/purchases")]
[Authorize(Roles = "explorer")]
public sealed class PurchaseViewingController : ControllerBase
{
    private readonly PurchaseViewingQueries _purchaseQueries;

    public PurchaseViewingController(PurchaseViewingQueries purchaseQueries)
    {
        _purchaseQueries = purchaseQueries;
    }

    [HttpGet("tours")]
    public async Task<ActionResult<PageResult<PurchasedTourDto>>> GetPurchasedTours([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _purchaseQueries.GetPurchasedToursAsync(User.GetUserId(), page, pageSize);
    }
}
