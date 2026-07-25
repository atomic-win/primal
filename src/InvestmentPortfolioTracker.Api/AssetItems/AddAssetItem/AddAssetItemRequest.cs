using System.Security.Claims;
using FastEndpoints;
using InvestmentPortfolioTracker.Domain.Investments;
using InvestmentPortfolioTracker.Domain.Money;

namespace InvestmentPortfolioTracker.Api.AssetItems;

internal sealed record AddAssetItemRequest(
	[property: FromClaim(ClaimTypes.NameIdentifier)] Guid UserId,
	string Name,
	AssetClass AssetClass,
	AssetType AssetType,
	string ExternalId,
	Currency Currency);
