namespace Payment.Application.CouponManagement;

public sealed record CreateCouponDto(Guid? ProductId, int Percent, DateOnly ExpiresOn);
