namespace Payment.Contracts;

public interface IPaymentApi
{
    Task<bool> HasPurchasedAsync(Guid buyerId, Guid tourId);
}
