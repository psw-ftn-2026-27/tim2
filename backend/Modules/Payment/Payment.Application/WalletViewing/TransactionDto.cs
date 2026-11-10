namespace Payment.Application.WalletViewing;

public sealed record TransactionDto(Guid Id, int Amount, DateTime CreatedAt, bool IsCancelled);
