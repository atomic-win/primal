namespace Primal.Contracts.Investments;

public abstract record InstrumentResponse(Guid Id, string Name, string Category, string Type);
