namespace Primal.Contracts.Investments;

public sealed record AddCashAssetRequest(
	string Name,
	string Type,
	string Currency);
