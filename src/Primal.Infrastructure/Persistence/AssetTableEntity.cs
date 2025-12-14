using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Persistence;

internal sealed class AssetTableEntity : TableEntity
{
	required internal Guid Id { get; set; }

	required internal string Name { get; set; }

	required internal AssetClass AssetClass { get; set; }

	required internal AssetType AssetType { get; set; }

	required internal Currency Currency { get; set; }

	required internal string ExternalId { get; set; }
}
