using Payment.Application.CouponManagement;

namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<List<CouponDto>> GetByOwnerAsync(Guid ownerId);
}
