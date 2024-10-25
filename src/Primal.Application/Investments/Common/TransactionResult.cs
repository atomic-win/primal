using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record TransactionResult(
	TransactionId Id,
	DateOnly Date,
	string Name,
	TransactionType Type,
	decimal Units,
	decimal Amount);
