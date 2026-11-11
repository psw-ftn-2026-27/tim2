namespace Payment.Application.WalletFunding;

public sealed record UserTransactionDto(Guid Id, int Amount, DateTime CreatedAt, Guid AdminId, bool IsCancelled);
