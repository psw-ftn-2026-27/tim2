using Exploration.Contracts;
using Payment.Application.Coupons;

namespace Payment.Application.CouponManagement;

public sealed class CouponManagementQueries
{
    private readonly ICouponReadRepository _couponReadRepository;
    private readonly IExplorationApi _explorationApi;

    public CouponManagementQueries(ICouponReadRepository couponReadRepository, IExplorationApi explorationApi)
    {
        _couponReadRepository = couponReadRepository;
        _explorationApi = explorationApi;
    }

    public async Task<List<CouponDto>> GetMyCouponsAsync(Guid ownerId)
    {
        var coupons = await _couponReadRepository.GetByOwnerAsync(ownerId);
        var tours = await _explorationApi.GetPublishedToursByAuthorAsync(ownerId);
        var tourNames = tours.ToDictionary(tour => tour.TourId, tour => tour.Name);

        return coupons
            .Select(coupon => coupon with
            {
                ProductName = coupon.ProductId is null ? null : tourNames.GetValueOrDefault(coupon.ProductId.Value, string.Empty)
            })
            .ToList();
    }

    public async Task<List<CouponProductDto>> GetMyProductsAsync(Guid ownerId)
    {
        var tours = await _explorationApi.GetPublishedToursByAuthorAsync(ownerId);

        return tours.Select(tour => new CouponProductDto(tour.TourId, tour.Name)).ToList();
    }
}
