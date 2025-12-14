using Primal.Domain.Investments;

namespace Primal.Infrastructure.Persistence;

internal sealed class TransactionTableEntity : TableEntity
{
	required internal Guid Id { get; init; }

	required internal DateOnly Date { get; init; }

	required internal string Name { get; init; }

	required internal TransactionType TransactionType { get; init; }

	required internal Guid AssetItemId { get; init; }

	required internal Guid UserId { get; init; }

	required internal decimal Units { get; init; }
}
