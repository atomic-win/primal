using Primal.Domain.Investments;

namespace Primal.Infrastructure.Persistence;

internal sealed class TransactionTableEntity : TableEntity
{
	internal required Guid Id { get; init; }

	internal required DateOnly Date { get; init; }

	internal required string Name { get; init; }

	internal required TransactionType TransactionType { get; init; }

	internal required Guid AssetItemId { get; init; }

	internal required Guid UserId { get; init; }

	internal required decimal Units { get; init; }
}
