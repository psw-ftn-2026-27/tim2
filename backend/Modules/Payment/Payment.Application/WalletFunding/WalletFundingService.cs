using Identity.Contracts;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletFunding;

public sealed class WalletFundingService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public WalletFundingService(IWalletRepository walletRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task DepositAsync(Guid adminId, Guid userId, DepositDto dto)
    {
        var explorer = await _identityApi.GetExplorerAsync(userId);
        if (explorer is null)
        {
            throw new NotFoundException($"User {userId} does not exist.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            wallet = new Wallet(userId);
            _walletRepository.Add(wallet);
        }

        wallet.Deposit(adminId, dto.Amount);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelDepositAsync(Guid adminId, Guid userId, Guid transactionId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            throw new NotFoundException($"Wallet of user {userId} does not exist.");
        }

        wallet.CancelDeposit(transactionId, adminId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }
}
