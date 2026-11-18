using Exploration.Contracts;
using Payment.Application.Coupons;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;

namespace Payment.Application.CouponManagement;

public sealed class CouponManagementService
{
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExplorationApi _explorationApi;

    public CouponManagementService(ICouponRepository couponRepository, IUnitOfWork unitOfWork, IExplorationApi explorationApi)
    {
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
        _explorationApi = explorationApi;
    }

    public async Task<Guid> CreateCouponAsync(Guid ownerId, CreateCouponDto dto)
    {
        Guid? productOwnerId = null;
        if (dto.ProductId is not null)
        {
            var tour = await _explorationApi.GetPublishedTourAsync(dto.ProductId.Value);
            if (tour is null)
            {
                throw new NotFoundException($"Published tour {dto.ProductId} does not exist.");
            }
            productOwnerId = tour.AuthorId;
        }

        var coupon = new Coupon(ownerId, dto.ProductId, productOwnerId, dto.Percent, dto.ExpiresOn, DateTime.UtcNow);
        _couponRepository.Add(coupon);
        await _unitOfWork.SaveChangesAsync();

        return coupon.Id;
    }
}
