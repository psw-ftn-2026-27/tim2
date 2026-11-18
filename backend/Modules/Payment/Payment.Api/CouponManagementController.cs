using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.CouponManagement;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/coupons")]
[Authorize(Roles = "explorer")]
public sealed class CouponManagementController : ControllerBase
{
    private readonly CouponManagementService _couponService;
    private readonly CouponManagementQueries _couponQueries;

    public CouponManagementController(CouponManagementService couponService, CouponManagementQueries couponQueries)
    {
        _couponService = couponService;
        _couponQueries = couponQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCouponDto dto)
    {
        return await _couponService.CreateCouponAsync(User.GetUserId(), dto);
    }

    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> GetMyCoupons()
    {
        return await _couponQueries.GetMyCouponsAsync(User.GetUserId());
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<CouponProductDto>>> GetMyProducts()
    {
        return await _couponQueries.GetMyProductsAsync(User.GetUserId());
    }
}
