using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed record AssetItemResponse(
	Guid Id,
	string Name,
	AssetType AssetType,
	AssetClass AssetClass,
	Currency Currency)
{
	internal static AssetItemResponse From(AssetItem assetItem, Asset asset) =>
		new(assetItem.Id.Value, assetItem.Name, asset.AssetType, asset.AssetClass, asset.Currency);
}
