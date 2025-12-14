using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal sealed record TransactionRequest(
	DateOnly Date,
	string Name,
	TransactionType TransactionType,
	Guid AssetItemId,
	decimal Units);
