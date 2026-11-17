namespace Payment.Application.Shopping;

public sealed record CartDto(List<CartItemDto> Items, int TotalPrice);
