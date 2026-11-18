using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCardGifting;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardGiftingController : ControllerBase
{
    private readonly GiftCardGiftingService _giftingService;
    private readonly GiftCardGiftingQueries _giftingQueries;

    public GiftCardGiftingController(GiftCardGiftingService giftingService, GiftCardGiftingQueries giftingQueries)
    {
        _giftingService = giftingService;
        _giftingQueries = giftingQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Purchase([FromBody] PurchaseGiftCardDto dto)
    {
        return await _giftingService.PurchaseAsync(User.GetUserId(), dto);
    }

    [HttpGet]
    public async Task<ActionResult<GiftCardOverviewDto>> GetOverview()
    {
        return await _giftingQueries.GetOverviewAsync(User.GetUserId());
    }
}
