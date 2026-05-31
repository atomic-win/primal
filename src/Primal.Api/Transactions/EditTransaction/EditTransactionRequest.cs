using System.Security.Claims;
using FastEndpoints;
using Primal.Domain.Investments;

namespace Primal.Api.Transactions;

internal sealed record EditTransactionRequest(
	Guid AssetItemId,
	Guid TransactionId,
	string Name,
	TransactionType TransactionType,
	decimal Units,
	decimal Price,
	decimal Amount)
{
	[FromClaim(ClaimTypes.NameIdentifier)]
	public Guid UserId { get; set; }
}
