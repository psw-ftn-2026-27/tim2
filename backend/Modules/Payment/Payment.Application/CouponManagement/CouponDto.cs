namespace Payment.Application.CouponManagement;

public sealed record CouponDto(Guid Id, string Code, int Percent, DateOnly ExpiresOn, Guid? ProductId, string? ProductName);
