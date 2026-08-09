using TravelInsurance.Application.AgencyFinance.Abstractions;
using TravelInsurance.Application.AgencyFinance.DTOs;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Payments.DTOs;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.AgencyFinance.Services;

public class AgencyFinanceService(
    IAgencyFinanceRepository repository,
    IPaymentService paymentService,
    INumberSequenceService numberSequenceService) : IAgencyFinanceService
{
    public async Task<(FundingAuthorizationResponse? Funding, OperationResponse Result)> AuthorizeForIssuanceAsync(PolicyIssuanceRequest issuance, PaymentSettlementInput settlement, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetFundingByIssuanceAsync(issuance.Id, cancellationToken);
        if (existing is not null) return (Map(existing), Ok("Existing funding authorization returned."));
        var mode = ToFundingMode(settlement.Method);
        if (mode is null) return (null, Fail("The selected payment method is not supported for policy funding."));

        if (mode != AgencyFundingMode.ElectronicGateway)
        {
            if (!issuance.AgencyCompanyId.HasValue) return (null, Fail("Agency wallet and postpaid credit require an agency issuance."));
            var profile = await repository.GetFundingProfileAsync(issuance.AgencyCompanyId.Value, mode.Value, cancellationToken);
            if (profile is null || !profile.IsEnabled) return (null, Fail("This funding method is not enabled for the agency."));
            if (profile.MaximumTransactionAmount.HasValue && issuance.GrossPremium > profile.MaximumTransactionAmount.Value) return (null, Fail("The premium exceeds the agency funding transaction limit."));
        }

        var funding = new PolicyFunding
        {
            IssuanceRequestId = issuance.Id,
            AgencyCompanyId = issuance.AgencyCompanyId,
            FundingMode = mode.Value,
            Amount = issuance.GrossPremium,
            Currency = NormalizedCurrency(issuance.Currency),
            FundingReference = $"FND-{issuance.Id:N}"
        };

        if (mode == AgencyFundingMode.AgencyWallet)
        {
            var wallet = await repository.GetWalletAsync(issuance.AgencyCompanyId!.Value, funding.Currency, cancellationToken);
            if (wallet is null || !wallet.IsActive) return (null, Fail("No active wallet exists for this agency and currency."));
            if (wallet.Balance - wallet.ReservedBalance < funding.Amount) return (null, Fail("The agency wallet has insufficient available balance."));
            wallet.ReservedBalance += funding.Amount; wallet.UpdatedAt = DateTimeOffset.UtcNow;
            repository.Add(new WalletTransaction { WalletId = wallet.Id, IssuanceRequestId = issuance.Id, Type = WalletTransactionType.Reservation, Amount = funding.Amount, BalanceBefore = wallet.Balance, BalanceAfter = wallet.Balance, Reference = funding.FundingReference });
            funding.Status = PolicyFundingStatus.Reserved;
            repository.Add(funding);
            return await SaveFundingAsync(funding, cancellationToken, "Wallet funds reserved for issuance.");
        }

        if (mode == AgencyFundingMode.PostpaidCredit)
        {
            var account = await repository.GetCreditAccountAsync(issuance.AgencyCompanyId!.Value, funding.Currency, cancellationToken);
            if (account is null || account.IsSuspended) return (null, Fail("No active postpaid credit account exists for this agency and currency."));
            if (account.CreditLimit - account.OutstandingAmount - account.ReservedAmount < funding.Amount) return (null, Fail("The agency has insufficient available credit."));
            account.ReservedAmount += funding.Amount; account.UpdatedAt = DateTimeOffset.UtcNow;
            repository.Add(new AgencyCreditTransaction { CreditAccountId = account.Id, IssuanceRequestId = issuance.Id, Type = CreditTransactionType.CreditReserved, Amount = funding.Amount, OutstandingBefore = account.OutstandingAmount, OutstandingAfter = account.OutstandingAmount, Reference = funding.FundingReference });
            funding.Status = PolicyFundingStatus.Reserved;
            repository.Add(funding);
            return await SaveFundingAsync(funding, cancellationToken, "Credit reserved for issuance.");
        }

        repository.Add(funding);
        if (await repository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Funding authorization conflicted. Please retry."));
        var (payment, result) = await paymentService.CreateForIssuanceAsync(issuance.Id, settlement, actorUserId, $"funding-payment:{idempotencyKey}", cancellationToken);
        if (!result.Success || payment is null)
        {
            funding.Status = PolicyFundingStatus.Failed;
            await repository.SaveChangesAsync(cancellationToken);
            return (null, Fail($"Electronic payment could not be started: {result.Message}"));
        }
        funding.PaymentId = payment.Payment.Id;
        if (payment.Payment.Status == PaymentStatus.Paid) funding.Status = PolicyFundingStatus.Funded;
        if (await repository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Funding authorization conflicted. Please retry."));
        return (Map(funding), Ok("Electronic payment started."));
    }

    public async Task<OperationResponse> ConfirmElectronicPaymentAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default)
    {
        var funding = await repository.GetFundingByIssuanceAsync(issuanceRequestId, cancellationToken);
        if (funding is null || funding.FundingMode != AgencyFundingMode.ElectronicGateway) return Fail("Policy funding was not found.");
        if (funding.Status == PolicyFundingStatus.Funded) return Ok("Electronic funding was already confirmed.");
        if (funding.Status is PolicyFundingStatus.Released or PolicyFundingStatus.Refunded) return Fail("Released funding cannot be confirmed.");
        funding.Status = PolicyFundingStatus.Funded;
        funding.FundedAt = DateTimeOffset.UtcNow;
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? Ok("Electronic funding confirmed.") : Fail("Funding confirmation conflicted. Please retry.");
    }

    public async Task<OperationResponse> CaptureForIssuedPolicyAsync(Guid issuanceRequestId, Guid policyId, CancellationToken cancellationToken = default)
    {
        var funding = await repository.GetFundingByIssuanceAsync(issuanceRequestId, cancellationToken);
        if (funding is null) return Fail("No funding authorization exists for this issuance.");
        if (funding.Status is PolicyFundingStatus.Released or PolicyFundingStatus.Failed) return Fail("Funding authorization is no longer valid.");
        if (funding.PolicyId == policyId && funding.Status == PolicyFundingStatus.Funded) return Ok("Funding was already captured.");

        if (funding.FundingMode == AgencyFundingMode.AgencyWallet)
        {
            var wallet = await repository.GetWalletAsync(funding.AgencyCompanyId!.Value, funding.Currency, cancellationToken);
            if (wallet is null || wallet.ReservedBalance < funding.Amount || wallet.Balance < funding.Amount) return Fail("Wallet reservation is unavailable.");
            var balanceBefore = wallet.Balance;
            wallet.ReservedBalance -= funding.Amount; wallet.Balance -= funding.Amount; wallet.UpdatedAt = DateTimeOffset.UtcNow;
            repository.Add(new WalletTransaction { WalletId = wallet.Id, IssuanceRequestId = issuanceRequestId, PolicyId = policyId, Type = WalletTransactionType.PolicyDebit, Amount = funding.Amount, BalanceBefore = balanceBefore, BalanceAfter = wallet.Balance, Reference = $"{funding.FundingReference}:CAPTURE" });
        }
        else if (funding.FundingMode == AgencyFundingMode.PostpaidCredit)
        {
            var account = await repository.GetCreditAccountAsync(funding.AgencyCompanyId!.Value, funding.Currency, cancellationToken);
            if (account is null || account.ReservedAmount < funding.Amount) return Fail("Credit reservation is unavailable.");
            var outstandingBefore = account.OutstandingAmount;
            account.ReservedAmount -= funding.Amount; account.OutstandingAmount += funding.Amount; account.UpdatedAt = DateTimeOffset.UtcNow;
            repository.Add(new AgencyCreditTransaction { CreditAccountId = account.Id, IssuanceRequestId = issuanceRequestId, PolicyId = policyId, Type = CreditTransactionType.PolicyIssued, Amount = funding.Amount, OutstandingBefore = outstandingBefore, OutstandingAfter = account.OutstandingAmount, Reference = $"{funding.FundingReference}:CAPTURE" });
        }
        funding.PolicyId = policyId; funding.Status = PolicyFundingStatus.Funded; funding.FundedAt ??= DateTimeOffset.UtcNow;
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? Ok("Funding captured.") : Fail("Funding capture conflicted. Please retry.");
    }

    public async Task<OperationResponse> ReleaseForFailedIssuanceAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default)
    {
        var funding = await repository.GetFundingByIssuanceAsync(issuanceRequestId, cancellationToken);
        if (funding is null || funding.Status is PolicyFundingStatus.Released or PolicyFundingStatus.Failed) return Ok("No active funding reservation exists.");
        if (funding.FundingMode == AgencyFundingMode.AgencyWallet && funding.Status == PolicyFundingStatus.Reserved)
        {
            var wallet = await repository.GetWalletAsync(funding.AgencyCompanyId!.Value, funding.Currency, cancellationToken);
            if (wallet is not null)
            {
                wallet.ReservedBalance -= funding.Amount; wallet.UpdatedAt = DateTimeOffset.UtcNow;
                repository.Add(new WalletTransaction { WalletId = wallet.Id, IssuanceRequestId = issuanceRequestId, Type = WalletTransactionType.ReservationReleased, Amount = funding.Amount, BalanceBefore = wallet.Balance, BalanceAfter = wallet.Balance, Reference = $"{funding.FundingReference}:RELEASE" });
            }
        }
        else if (funding.FundingMode == AgencyFundingMode.PostpaidCredit && funding.Status == PolicyFundingStatus.Reserved)
        {
            var account = await repository.GetCreditAccountAsync(funding.AgencyCompanyId!.Value, funding.Currency, cancellationToken);
            if (account is not null)
            {
                account.ReservedAmount -= funding.Amount; account.UpdatedAt = DateTimeOffset.UtcNow;
                repository.Add(new AgencyCreditTransaction { CreditAccountId = account.Id, IssuanceRequestId = issuanceRequestId, Type = CreditTransactionType.ReservationReleased, Amount = funding.Amount, OutstandingBefore = account.OutstandingAmount, OutstandingAfter = account.OutstandingAmount, Reference = $"{funding.FundingReference}:RELEASE" });
            }
        }
        funding.Status = funding.FundingMode == AgencyFundingMode.ElectronicGateway && funding.Status == PolicyFundingStatus.Funded ? PolicyFundingStatus.Failed : PolicyFundingStatus.Released;
        funding.ReleasedAt = DateTimeOffset.UtcNow;
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? Ok("Funding released.") : Fail("Funding release conflicted. Please retry.");
    }

    public async Task<(IReadOnlyCollection<FundingProfileResponse> Profiles, OperationResponse Result)> GetProfilesAsync(Guid agencyCompanyId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanViewAsync(agencyCompanyId, actorUserId, cancellationToken)) return (Array.Empty<FundingProfileResponse>(), Fail("Agency finance profile was not found."));
        var profiles = await repository.GetFundingProfilesAsync(agencyCompanyId, cancellationToken);
        return (profiles.Select(Map).ToArray(), Ok("Funding profiles found."));
    }

    public async Task<OperationResponse> ConfigureProfileAsync(Guid agencyCompanyId, ConfigureFundingProfileRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanManageAsync(actorUserId, cancellationToken) || !await repository.IsAgencyAsync(agencyCompanyId, cancellationToken)) return Fail("Agency funding profile was not found.");
        var profile = await repository.GetFundingProfileAsync(agencyCompanyId, request.Mode, cancellationToken);
        if (profile is null) { profile = new AgencyFundingProfile { AgencyCompanyId = agencyCompanyId, Mode = request.Mode }; repository.Add(profile); }
        if (request.IsDefault)
        {
            var profiles = await repository.GetFundingProfilesAsync(agencyCompanyId, cancellationToken);
            foreach (var item in profiles.Where(x => x.Id != profile.Id && x.IsDefault)) item.IsDefault = false;
        }
        profile.IsDefault = request.IsDefault; profile.IsEnabled = request.IsEnabled; profile.MaximumTransactionAmount = request.MaximumTransactionAmount; profile.UpdatedAt = DateTimeOffset.UtcNow;
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? Ok("Agency funding profile saved.") : Fail("Funding profile conflicted. Please retry.");
    }

    public async Task<(WalletResponse? Wallet, OperationResponse Result)> GetWalletAsync(Guid agencyCompanyId, string currency, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanViewAsync(agencyCompanyId, actorUserId, cancellationToken)) return (null, Fail("Agency wallet was not found."));
        var wallet = await repository.GetWalletAsync(agencyCompanyId, NormalizedCurrency(currency), cancellationToken);
        return wallet is null ? (null, Fail("Agency wallet was not found.")) : (Map(wallet), Ok("Agency wallet found."));
    }

    public async Task<(WalletResponse? Wallet, OperationResponse Result)> TopUpWalletAsync(Guid agencyCompanyId, WalletTopUpRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanManageAsync(actorUserId, cancellationToken) || !await repository.IsAgencyAsync(agencyCompanyId, cancellationToken)) return (null, Fail("Agency wallet was not found."));
        var currency = NormalizedCurrency(request.Currency);
        var wallet = await repository.GetWalletAsync(agencyCompanyId, currency, cancellationToken);
        if (wallet is null) { wallet = new AgencyWallet { AgencyCompanyId = agencyCompanyId, Currency = currency }; repository.Add(wallet); }
        var before = wallet.Balance; wallet.Balance += request.Amount; wallet.UpdatedAt = DateTimeOffset.UtcNow;
        var reference = await numberSequenceService.NextAsync(agencyCompanyId, NumberSequenceType.Payment, cancellationToken);
        repository.Add(new WalletTransaction { WalletId = wallet.Id, Type = WalletTransactionType.TopUp, Amount = request.Amount, BalanceBefore = before, BalanceAfter = wallet.Balance, Reference = reference, Notes = request.Notes });
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? (Map(wallet), Ok("Agency wallet credited.")) : (null, Fail("Wallet credit conflicted. Please retry."));
    }

    public async Task<(CreditAccountResponse? Account, OperationResponse Result)> ConfigureCreditAsync(Guid agencyCompanyId, ConfigureCreditAccountRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanManageAsync(actorUserId, cancellationToken) || !await repository.IsAgencyAsync(agencyCompanyId, cancellationToken)) return (null, Fail("Agency credit account was not found."));
        var currency = NormalizedCurrency(request.Currency);
        var account = await repository.GetCreditAccountAsync(agencyCompanyId, currency, cancellationToken);
        if (account is null) { account = new AgencyCreditAccount { AgencyCompanyId = agencyCompanyId, Currency = currency }; repository.Add(account); }
        if (request.CreditLimit < account.OutstandingAmount + account.ReservedAmount) return (null, Fail("Credit limit cannot be lower than the outstanding and reserved credit."));
        account.CreditLimit = request.CreditLimit; account.BillingDay = request.BillingDay; account.PaymentDueDays = request.PaymentDueDays; account.GracePeriodDays = request.GracePeriodDays; account.AutoSuspendOnOverdue = request.AutoSuspendOnOverdue; account.IsSuspended = request.IsSuspended; account.UpdatedAt = DateTimeOffset.UtcNow;
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? (Map(account), Ok("Agency credit account saved.")) : (null, Fail("Credit account conflicted. Please retry."));
    }

    public async Task<(CreditAccountResponse? Account, OperationResponse Result)> GetCreditAsync(Guid agencyCompanyId, string currency, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanViewAsync(agencyCompanyId, actorUserId, cancellationToken)) return (null, Fail("Agency credit account was not found."));
        var account = await repository.GetCreditAccountAsync(agencyCompanyId, NormalizedCurrency(currency), cancellationToken);
        return account is null ? (null, Fail("Agency credit account was not found.")) : (Map(account), Ok("Agency credit account found."));
    }

    public async Task<(SettlementResponse? Settlement, OperationResponse Result)> CreateSettlementAsync(Guid agencyCompanyId, CreateSettlementRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanManageAsync(actorUserId, cancellationToken) || request.PeriodTo < request.PeriodFrom) return (null, Fail("Settlement request is invalid."));
        var currency = NormalizedCurrency(request.Currency);
        var account = await repository.GetCreditAccountAsync(agencyCompanyId, currency, cancellationToken);
        if (account is null) return (null, Fail("Agency credit account was not found."));
        var transactions = await repository.GetUnsettledCreditTransactionsAsync(agencyCompanyId, currency, request.PeriodFrom, request.PeriodTo, cancellationToken);
        if (transactions.Count == 0) return (null, Fail("No unsettled postpaid policy transactions were found for this period."));
        var gross = transactions.Sum(x => x.Amount); var net = gross - request.AgencyCommission;
        if (net < 0) return (null, Fail("Agency commission cannot exceed the policy premium total."));
        var settlement = new AgencySettlement { AgencyCompanyId = agencyCompanyId, Currency = currency, SettlementNumber = await numberSequenceService.NextAsync(agencyCompanyId, NumberSequenceType.Settlement, cancellationToken), PeriodFrom = request.PeriodFrom, PeriodTo = request.PeriodTo, GrossPremium = gross, AgencyCommission = request.AgencyCommission, NetAmountDue = net, RemainingAmount = net, DueDate = request.PeriodTo.AddDays(account.PaymentDueDays), Status = SettlementStatus.Issued, IssuedAt = DateTimeOffset.UtcNow };
        repository.Add(settlement);
        foreach (var transaction in transactions) transaction.SettlementId = settlement.Id;
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? (Map(settlement), Ok("Monthly settlement issued.")) : (null, Fail("Settlement conflicted. Please retry."));
    }

    public async Task<(SettlementResponse? Settlement, OperationResponse Result)> RecordSettlementPaymentAsync(Guid settlementId, RecordSettlementPaymentRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanManageAsync(actorUserId, cancellationToken)) return (null, Fail("Settlement was not found."));
        var settlement = await repository.GetSettlementAsync(settlementId, cancellationToken);
        if (settlement is null || settlement.Status is SettlementStatus.Cancelled or SettlementStatus.Paid || request.Amount > settlement.RemainingAmount) return (null, Fail("Settlement payment is invalid."));
        var account = await repository.GetCreditAccountAsync(settlement.AgencyCompanyId, settlement.Currency, cancellationToken);
        if (account is null || account.OutstandingAmount < request.Amount) return (null, Fail("Credit account balance is invalid."));
        var outstandingBefore = account.OutstandingAmount; account.OutstandingAmount -= request.Amount; account.UpdatedAt = DateTimeOffset.UtcNow;
        settlement.PaidAmount += request.Amount; settlement.RemainingAmount -= request.Amount; settlement.Status = settlement.RemainingAmount == 0 ? SettlementStatus.Paid : SettlementStatus.PartiallyPaid;
        var reference = await numberSequenceService.NextAsync(settlement.AgencyCompanyId, NumberSequenceType.Payment, cancellationToken);
        repository.Add(new AgencyCreditTransaction { CreditAccountId = account.Id, SettlementId = settlement.Id, Type = CreditTransactionType.PaymentReceived, Amount = request.Amount, OutstandingBefore = outstandingBefore, OutstandingAfter = account.OutstandingAmount, Reference = reference });
        return await repository.SaveChangesAsync(cancellationToken) == WorkflowSaveResult.Saved ? (Map(settlement), Ok("Settlement payment recorded.")) : (null, Fail("Settlement payment conflicted. Please retry."));
    }

    private async Task<(FundingAuthorizationResponse? Funding, OperationResponse Result)> SaveFundingAsync(PolicyFunding funding, CancellationToken ct, string message) =>
        await repository.SaveChangesAsync(ct) == WorkflowSaveResult.Saved ? (Map(funding), Ok(message)) : (null, Fail("Funding authorization conflicted. Please retry."));
    private async Task<bool> CanManageAsync(Guid actorUserId, CancellationToken ct) { var actor = await repository.GetActorAsync(actorUserId, ct); return actor is not null && (actor.IsSuperAdmin || actor.CompanyType == CompanyType.Insurance); }
    private async Task<bool> CanViewAsync(Guid agencyCompanyId, Guid actorUserId, CancellationToken ct) { var actor = await repository.GetActorAsync(actorUserId, ct); return actor is not null && (actor.IsSuperAdmin || actor.CompanyType == CompanyType.Insurance || actor.CompanyId == agencyCompanyId); }
    private static AgencyFundingMode? ToFundingMode(PaymentMethod method) => method switch { PaymentMethod.ElectronicGateway => AgencyFundingMode.ElectronicGateway, PaymentMethod.AgencyWallet or PaymentMethod.PrepaidCredit => AgencyFundingMode.AgencyWallet, PaymentMethod.PostpaidCredit => AgencyFundingMode.PostpaidCredit, _ => null };
    private static string NormalizedCurrency(string currency) => currency.Trim().ToUpperInvariant();
    private static FundingAuthorizationResponse Map(PolicyFunding x) => new(x.Id, x.PaymentId, x.FundingMode, x.Status, x.Amount, x.Currency, x.FundingReference);
    private static FundingProfileResponse Map(AgencyFundingProfile x) => new(x.Id, x.Mode, x.IsDefault, x.IsEnabled, x.MaximumTransactionAmount);
    private static WalletResponse Map(AgencyWallet x) => new(x.Id, x.AgencyCompanyId, x.Currency, x.Balance, x.ReservedBalance, x.Balance - x.ReservedBalance, x.IsActive);
    private static CreditAccountResponse Map(AgencyCreditAccount x) => new(x.Id, x.AgencyCompanyId, x.Currency, x.CreditLimit, x.OutstandingAmount, x.ReservedAmount, x.CreditLimit - x.OutstandingAmount - x.ReservedAmount, x.IsSuspended, x.BillingDay, x.PaymentDueDays, x.GracePeriodDays);
    private static SettlementResponse Map(AgencySettlement x) => new(x.Id, x.SettlementNumber, x.AgencyCompanyId, x.Currency, x.PeriodFrom, x.PeriodTo, x.GrossPremium, x.AgencyCommission, x.Refunds, x.NetAmountDue, x.PaidAmount, x.RemainingAmount, x.DueDate, x.Status);
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}

