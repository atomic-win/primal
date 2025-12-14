using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal sealed record TransactionResponse(
	Guid Id,
	DateOnly Date,
	string Name,
	TransactionType TransactionType,
	Guid AssetItemId,
	decimal Units,
	decimal Amount);
