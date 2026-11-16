namespace Payment.Application.Shopping;

public sealed record CartItemDto(Guid TourId, string Name, int Price);
