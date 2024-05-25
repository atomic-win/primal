namespace Primal.Contracts.Investments;

public sealed record BuySellRequest(DateOnly Date, string Name, string Type, Guid AssetId, decimal Units);
