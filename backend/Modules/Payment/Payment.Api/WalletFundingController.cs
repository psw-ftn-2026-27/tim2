using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletFunding;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize(Roles = "administrator")]
public sealed class WalletFundingController : ControllerBase
{
    private readonly WalletFundingService _fundingService;
    private readonly WalletFundingQueries _fundingQueries;

    public WalletFundingController(WalletFundingService fundingService, WalletFundingQueries fundingQueries)
    {
        _fundingService = fundingService;
        _fundingQueries = fundingQueries;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PageResult<UserWalletDto>>> GetWallets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _fundingQueries.GetWalletsAsync(page, pageSize);
    }

    [HttpPost("users/{userId:guid}/deposits")]
    public async Task<ActionResult> Deposit(Guid userId, DepositDto dto)
    {
        await _fundingService.DepositAsync(User.GetUserId(), userId, dto);
        return NoContent();
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<UserWalletDto>> GetWallet(Guid userId)
    {
        return await _fundingQueries.GetWalletAsync(userId);
    }

    [HttpGet("users/{userId:guid}/transactions")]
    public async Task<ActionResult<PageResult<UserTransactionDto>>> GetWalletTransactions(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _fundingQueries.GetWalletTransactionsAsync(userId, page, pageSize);
    }

    [HttpPost("users/{userId:guid}/transactions/{transactionId:guid}/cancellation")]
    public async Task<ActionResult> CancelDeposit(Guid userId, Guid transactionId)
    {
        await _fundingService.CancelDepositAsync(User.GetUserId(), userId, transactionId);
        return NoContent();
    }
}
