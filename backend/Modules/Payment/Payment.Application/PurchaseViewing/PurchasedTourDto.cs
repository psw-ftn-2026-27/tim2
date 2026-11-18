namespace Payment.Application.PurchaseViewing;

public sealed record PurchasedTourDto(Guid TourId, string Name, int Price, DateTime PurchasedAt);
