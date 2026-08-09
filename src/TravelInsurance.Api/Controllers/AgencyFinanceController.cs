using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.AgencyFinance.Abstractions;
using TravelInsurance.Application.AgencyFinance.DTOs;
using TravelInsurance.Application.Authentication.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("agency-finance")]
[Authorize]
public class AgencyFinanceController(IAgencyFinanceService agencyFinanceService) : ControllerBase
{
    [HttpGet("agencies/{agencyId:guid}/funding-profiles")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<ActionResult<IReadOnlyCollection<FundingProfileResponse>>> GetProfiles(Guid agencyId, CancellationToken cancellationToken)
    {
        var (profiles, result) = await agencyFinanceService.GetProfilesAsync(agencyId, User.GetUserId()!.Value, cancellationToken);
        return result.Success ? Ok(profiles) : NotFound(result);
    }

    [HttpPut("agencies/{agencyId:guid}/funding-profiles")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<IActionResult> ConfigureProfile(Guid agencyId, ConfigureFundingProfileRequest request, CancellationToken cancellationToken) =>
        Result(await agencyFinanceService.ConfigureProfileAsync(agencyId, request, User.GetUserId()!.Value, cancellationToken));

    [HttpGet("agencies/{agencyId:guid}/wallet/{currency}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<ActionResult<WalletResponse>> GetWallet(Guid agencyId, string currency, CancellationToken cancellationToken)
    {
        var (wallet, result) = await agencyFinanceService.GetWalletAsync(agencyId, currency, User.GetUserId()!.Value, cancellationToken);
        return result.Success && wallet is not null ? Ok(wallet) : NotFound(result);
    }

    [HttpPost("agencies/{agencyId:guid}/wallet/top-ups")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<ActionResult<WalletResponse>> TopUpWallet(Guid agencyId, WalletTopUpRequest request, CancellationToken cancellationToken)
    {
        var (wallet, result) = await agencyFinanceService.TopUpWalletAsync(agencyId, request, User.GetUserId()!.Value, cancellationToken);
        return result.Success && wallet is not null ? Ok(wallet) : BadRequest(result);
    }

    [HttpGet("agencies/{agencyId:guid}/credit/{currency}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<ActionResult<CreditAccountResponse>> GetCredit(Guid agencyId, string currency, CancellationToken cancellationToken)
    {
        var (account, result) = await agencyFinanceService.GetCreditAsync(agencyId, currency, User.GetUserId()!.Value, cancellationToken);
        return result.Success && account is not null ? Ok(account) : NotFound(result);
    }

    [HttpPut("agencies/{agencyId:guid}/credit")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<ActionResult<CreditAccountResponse>> ConfigureCredit(Guid agencyId, ConfigureCreditAccountRequest request, CancellationToken cancellationToken)
    {
        var (account, result) = await agencyFinanceService.ConfigureCreditAsync(agencyId, request, User.GetUserId()!.Value, cancellationToken);
        return result.Success && account is not null ? Ok(account) : BadRequest(result);
    }

    [HttpPost("agencies/{agencyId:guid}/settlements")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<ActionResult<SettlementResponse>> CreateSettlement(Guid agencyId, CreateSettlementRequest request, CancellationToken cancellationToken)
    {
        var (settlement, result) = await agencyFinanceService.CreateSettlementAsync(agencyId, request, User.GetUserId()!.Value, cancellationToken);
        return result.Success && settlement is not null ? Ok(settlement) : BadRequest(result);
    }

    [HttpPost("settlements/{settlementId:guid}/payments")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "AGENCY.FINANCE.MANAGE")]
    public async Task<ActionResult<SettlementResponse>> RecordSettlementPayment(Guid settlementId, RecordSettlementPaymentRequest request, CancellationToken cancellationToken)
    {
        var (settlement, result) = await agencyFinanceService.RecordSettlementPaymentAsync(settlementId, request, User.GetUserId()!.Value, cancellationToken);
        return result.Success && settlement is not null ? Ok(settlement) : BadRequest(result);
    }

    private static IActionResult Result(OperationResponse result) => result.Success ? new OkObjectResult(result) : new BadRequestObjectResult(result);
}
