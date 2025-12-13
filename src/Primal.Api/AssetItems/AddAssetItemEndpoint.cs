using FastEndpoints;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

[HttpPost("/api/assetItems")]
internal sealed class AddAssetItemEndpoint : Endpoint<AssetItemRequest>
{
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IStockApiClient stockApiClient;

	private readonly IAssetRepository assetRepository;
	private readonly IAssetItemRepository assetItemRepository;

	public AddAssetItemEndpoint(
		IMutualFundApiClient mutualFundApiClient,
		IStockApiClient stockApiClient,
		IAssetRepository assetRepository,
		IAssetItemRepository assetItemRepository)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.stockApiClient = stockApiClient;
		this.assetRepository = assetRepository;
		this.assetItemRepository = assetItemRepository;
	}

	public override async Task HandleAsync(AssetItemRequest req, CancellationToken ct)
	{
		this.ValidateRequest(req);

		var userId = this.GetUserId();

		if (req.AssetType == AssetType.MutualFund)
		{
			await this.AddMutualFundAsync(req, ct);
			return;
		}

		if (req.AssetType == AssetType.Stock)
		{
			await this.AddStockAsync(req, ct);
			return;
		}

		await this.AddOtherAssetItemTypeAsync(req, ct);
	}

	private async Task AddMutualFundAsync(AssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"mf-{req.ExternalId}", ct);

		if (asset.Id == AssetId.Empty)
		{
			var mutualFund = await this.mutualFundApiClient.GetByIdAsync(req.ExternalId, ct);

			if (string.IsNullOrWhiteSpace(mutualFund.SchemeCode))
			{
				this.ThrowError("Mutual fund not found", StatusCodes.Status404NotFound);
			}

			var assetClass = mutualFund switch
			{
				{ SchemeType: var st } when st != null && st.Contains("debt", StringComparison.CurrentCultureIgnoreCase) => AssetClass.Debt,
				{ SchemeCategory: var sc } when sc != null && sc.Contains("debt", StringComparison.CurrentCultureIgnoreCase) => AssetClass.Debt,
				_ => AssetClass.Equity,
			};

			asset = await this.assetRepository.AddAsync(
				mutualFund.Name,
				assetClass,
				AssetType.MutualFund,
				Currency.INR,
				$"mf-{mutualFund.SchemeCode}",
				ct);
		}

		await this.AddAssetItemAsync(asset.Id, req.Name, ct);
	}

	private async Task AddStockAsync(AssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"stock-{req.ExternalId.ToLowerInvariant()}", ct);
		if (asset.Id == AssetId.Empty)
		{
			var stock = await this.stockApiClient.GetByIdAsync(req.ExternalId, ct);

			if (string.IsNullOrWhiteSpace(stock.Symbol))
			{
				this.ThrowError("Stock not found", StatusCodes.Status404NotFound);
			}

			asset = await this.assetRepository.AddAsync(
				stock.Name,
				AssetClass.Equity,
				AssetType.Stock,
				Currency.USD,
				$"stock-{stock.Symbol.ToLowerInvariant()}",
				ct);
		}

		await this.AddAssetItemAsync(asset.Id, req.Name, ct);
	}

	private async Task AddOtherAssetItemTypeAsync(AssetItemRequest req, CancellationToken ct)
	{
		var asset = await this.assetRepository.GetByExternalIdAsync($"default-{req.AssetClass}-{req.AssetType}-{req.Currency}", ct);

		if (asset.Id == AssetId.Empty)
		{
			asset = await this.assetRepository.AddAsync(
				$"default-{req.AssetClass}-{req.AssetType}-{req.Currency}",
				req.AssetClass,
				req.AssetType,
				req.Currency,
				$"default-{req.AssetClass}-{req.AssetType}-{req.Currency}",
				ct);
		}

		await this.AddAssetItemAsync(asset.Id, req.Name, ct);
	}

	private async Task AddAssetItemAsync(
		AssetId assetId,
		string name,
		CancellationToken ct)
	{
		await this.assetItemRepository.AddAsync(
			this.GetUserId(),
			assetId,
			name,
			ct);
	}

	private void ValidateRequest(AssetItemRequest req)
	{
		if (req.AssetType == AssetType.Unknown)
		{
			this.ThrowError("Asset type cannot be Unknown", StatusCodes.Status400BadRequest);
		}

		if (string.IsNullOrWhiteSpace(req.Name))
		{
			this.AddError("Name cannot be empty");
		}

		if (req.AssetClass == AssetClass.Unknown)
		{
			if (req.AssetType != AssetType.MutualFund && req.AssetType != AssetType.Stock && req.AssetType != AssetType.Bond)
			{
				this.AddError($"Asset class must be specified for {req.AssetType} asset type");
			}
		}
		else
		{
			if (req.AssetType == AssetType.MutualFund || req.AssetType == AssetType.Stock || req.AssetType == AssetType.Bond)
			{
				this.AddError($"Asset class must not be specified for {req.AssetType} asset type");
			}
		}

		if (string.IsNullOrWhiteSpace(req.ExternalId))
		{
			if (req.AssetType == AssetType.MutualFund || req.AssetType == AssetType.Stock)
			{
				this.AddError($"ExternalId must be specified for {req.AssetType} asset type");
			}
		}
		else
		{
			if (req.AssetType != AssetType.MutualFund && req.AssetType != AssetType.Stock)
			{
				this.AddError($"ExternalId must not be specified for {req.AssetType} asset type");
			}
		}

		if (req.Currency == Currency.Unknown)
		{
			if (req.AssetType != AssetType.MutualFund && req.AssetType != AssetType.Stock)
			{
				this.AddError($"Currency must be specified for {req.AssetType} asset type");
			}
		}
		else
		{
			if (req.AssetType == AssetType.MutualFund || req.AssetType == AssetType.Stock)
			{
				this.AddError($"Currency must not be specified for {req.AssetType} asset type");
			}
		}

		this.ThrowIfAnyErrors(StatusCodes.Status400BadRequest);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
internal sealed record AssetItemRequest(
	string Name,
	AssetClass AssetClass,
	AssetType AssetType,
	string ExternalId,
	Currency Currency);
