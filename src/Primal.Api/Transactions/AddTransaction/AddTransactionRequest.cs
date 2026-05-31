using System.Security.Claims;
using FastEndpoints;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal sealed record AddTransactionRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	Guid AssetItemId,
	DateOnly Date,
	string Name,
	TransactionType TransactionType,
	decimal Units,
	decimal Price,
	decimal Amount);
