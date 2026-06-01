using FastEndpoints;
using FluentValidation;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.AssetItems;

internal sealed class AddAssetItemValidator : Validator<AddAssetItemRequest>
{
	public AddAssetItemValidator()
	{
		this.RuleFor(x => x.AssetType)
			.NotEqual(AssetType.Unknown)
			.WithMessage("Asset type cannot be Unknown")
			.WithErrorCode("ASSET_TYPE_UNKNOWN");

		this.RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Name cannot be empty")
			.WithErrorCode("NAME_REQUIRED");

		this.RuleFor(x => x.AssetClass)
			.Must((req, assetClass) => assetClass != AssetClass.Unknown)
			.When(req => req.AssetType != AssetType.Stock && req.AssetType != AssetType.Bond)
			.WithMessage(req => $"Asset class must be specified for {req.AssetType} asset type")
			.WithErrorCode("ASSET_CLASS_REQUIRED");

		this.RuleFor(x => x.AssetClass)
			.Equal(AssetClass.Unknown)
			.When(req => req.AssetType == AssetType.Stock || req.AssetType == AssetType.Bond)
			.WithMessage(req => $"Asset class must not be specified for {req.AssetType} asset type")
			.WithErrorCode("ASSET_CLASS_NOT_ALLOWED");

		this.RuleFor(x => x.AssetClass)
			.Must(assetClass =>
				assetClass == AssetClass.Equity ||
				assetClass == AssetClass.Debt ||
				assetClass == AssetClass.Commodities)
			.When(req => req.AssetType == AssetType.MutualFund)
			.WithMessage(req => $"Asset class '{req.AssetClass}' is not valid for MutualFund asset type")
			.WithErrorCode("ASSET_CLASS_INVALID");

		this.RuleFor(x => x.ExternalId)
			.NotEmpty()
			.When(req => req.AssetType == AssetType.MutualFund || req.AssetType == AssetType.Stock)
			.WithMessage(req => $"ExternalId must be specified for {req.AssetType} asset type")
			.WithErrorCode("EXTERNAL_ID_REQUIRED");

		this.RuleFor(x => x.ExternalId)
			.Empty()
			.When(req => req.AssetType != AssetType.MutualFund && req.AssetType != AssetType.Stock && req.AssetType != AssetType.Unknown)
			.WithMessage(req => $"ExternalId must not be specified for {req.AssetType} asset type")
			.WithErrorCode("EXTERNAL_ID_NOT_ALLOWED");

		this.RuleFor(x => x.Currency)
			.Must(currency => currency != Currency.Unknown)
			.When(req => req.AssetType != AssetType.MutualFund && req.AssetType != AssetType.Stock)
			.WithMessage(req => $"Currency must be specified for {req.AssetType} asset type")
			.WithErrorCode("CURRENCY_REQUIRED");

		this.RuleFor(x => x.Currency)
			.Equal(Currency.Unknown)
			.When(req => req.AssetType == AssetType.MutualFund || req.AssetType == AssetType.Stock)
			.WithMessage(req => $"Currency must not be specified for {req.AssetType} asset type")
			.WithErrorCode("CURRENCY_NOT_ALLOWED");
	}
}
