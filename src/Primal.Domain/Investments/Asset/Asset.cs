using Primal.Domain.Common.Models;

namespace Primal.Domain.Investments;

public sealed class Asset : Entity<AssetId>
{
	public Asset(AssetId id, string name, InstrumentId instrumentId)
		: base(id)
	{
		this.Name = name;
		this.InstrumentId = instrumentId;
	}

	public string Name { get; init; }

	public InstrumentId InstrumentId { get; init; }
}
