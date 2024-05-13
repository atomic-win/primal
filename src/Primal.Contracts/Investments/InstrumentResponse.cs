namespace Primal.Contracts.Investments;

public sealed record InstrumentResponse(Guid Id, string Name, string Category, string Type, string AccountId);
