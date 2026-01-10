using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal sealed record TransactionRequest(
	Guid AssetItemId,
	Guid TransactionId,
	DateOnly Date,
	string Name,
	TransactionType TransactionType,
	decimal Units,
	decimal Price,
	decimal Amount);
