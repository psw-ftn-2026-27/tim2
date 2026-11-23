namespace Payment.Application.WalletFunding;

public sealed record UserWalletDto(Guid UserId, string Email, int Balance);
