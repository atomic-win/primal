namespace Primal.Contracts.Investments;

public sealed record AddCashDepositAssetRequest(
	string Name,
	string Type,
	string Currency);
