using InvestmentPortfolioTracker.Domain.Common.Models;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Domain.Investments;

public sealed class Asset : Entity<AssetId>
{
	public Asset(
		AssetId id,
		string name,
		AssetClass assetClass,
		AssetType assetType,
		Currency currency,
		string externalId)
		: base(id)
	{
		this.Name = name;
		this.AssetClass = assetClass;
		this.AssetType = assetType;
		this.Currency = currency;
		this.ExternalId = externalId;
	}

	public static Asset Empty { get; } = new Asset(
		AssetId.Empty,
		string.Empty,
		AssetClass.Unknown,
		AssetType.Unknown,
		Currency.Unknown,
		string.Empty);

	public string Name { get; init; }

	public AssetClass AssetClass { get; init; }

	public AssetType AssetType { get; init; }

	public Currency Currency { get; init; }

	public string ExternalId { get; init; }
}
