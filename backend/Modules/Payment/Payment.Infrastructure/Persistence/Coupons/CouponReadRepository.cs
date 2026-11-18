using Microsoft.EntityFrameworkCore;
using Payment.Application.CouponManagement;
using Payment.Application.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<CouponDto>> GetByOwnerAsync(Guid ownerId)
    {
        return _dbContext.Coupons
            .Where(coupon => coupon.OwnerId == ownerId)
            .OrderBy(coupon => coupon.ExpiresOn)
            .ThenBy(coupon => coupon.Code)
            .Select(coupon => new CouponDto(coupon.Id, coupon.Code, coupon.Percent, coupon.ExpiresOn, coupon.ProductEntityId, null))
            .ToListAsync();
    }
}
