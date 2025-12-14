using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Persistence;

internal sealed class AssetTableEntity : TableEntity
{
	internal required Guid Id { get; set; }

	internal required string Name { get; set; }

	internal required AssetClass AssetClass { get; set; }

	internal required AssetType AssetType { get; set; }

	internal required Currency Currency { get; set; }

	internal required string ExternalId { get; set; }
}
