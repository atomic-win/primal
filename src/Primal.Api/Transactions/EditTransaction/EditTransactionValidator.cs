using FastEndpoints;
using FluentValidation;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Transactions;

internal sealed class EditTransactionValidator : Validator<EditTransactionRequest>
{
	private readonly IAssetItemRepository assetItemRepository;
	private readonly IAssetRepository assetRepository;
	private readonly ITransactionRepository transactionRepository;

	public EditTransactionValidator(
		IAssetItemRepository assetItemRepository,
		IAssetRepository assetRepository,
		ITransactionRepository transactionRepository)
	{
		this.assetItemRepository = assetItemRepository;
		this.assetRepository = assetRepository;
		this.transactionRepository = transactionRepository;

		this.ConfigureAssetItemRules();
		this.ConfigureTransactionExistsRule();
		this.ConfigureFieldRules();
		this.ConfigureAmountRules();
	}

	private void ConfigureAssetItemRules()
	{
		this.RuleFor(x => x.AssetItemId)
			.NotEqual(Guid.Empty)
			.WithMessage("Asset item ID must be provided.");

		this.RuleFor(x => x.AssetItemId)
			.MustAsync(async (req, assetItemId, ct) =>
			{
				var userId = new UserId(req.UserId);
				var assetItem = await this.assetItemRepository.GetByIdAsync(userId, new AssetItemId(assetItemId), ct);
				return assetItem.Id != AssetItemId.Empty;
			})
			.When(x => x.AssetItemId != Guid.Empty)
			.WithMessage("Asset item does not exist.");
	}

	private void ConfigureTransactionExistsRule()
	{
		this.RuleFor(x => x)
			.MustAsync(async (req, ct) =>
			{
				var userId = new UserId(req.UserId);
				var existingTransaction = await this.transactionRepository.GetByIdAsync(
					userId,
					new AssetItemId(req.AssetItemId),
					new TransactionId(req.TransactionId),
					ct);
				return existingTransaction.Id != TransactionId.Empty;
			})
			.When(x => x.AssetItemId != Guid.Empty && x.TransactionId != Guid.Empty)
			.WithMessage("Transaction does not exist.");
	}

	private void ConfigureFieldRules()
	{
		this.RuleFor(x => x.Name)
			.MinimumLength(3)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage("Transaction name must be at least 3 characters long.");

		this.RuleFor(x => x.Name)
			.MaximumLength(1000)
			.When(x => !string.IsNullOrWhiteSpace(x.Name))
			.WithMessage("Transaction name must not exceed 1000 characters.");

		this.RuleFor(x => x)
			.MustAsync(async (req, ct) =>
			{
				var userId = new UserId(req.UserId);
				var assetItem = await this.assetItemRepository.GetByIdAsync(userId, new AssetItemId(req.AssetItemId), ct);
				if (assetItem.Id == AssetItemId.Empty)
				{
					return true;
				}

				var asset = await this.assetRepository.GetByIdAsync(assetItem.AssetId, ct);
				return TransactionValidationExtensions.IsValidForAssetType(req.TransactionType, asset);
			})
			.When(x => x.AssetItemId != Guid.Empty && x.TransactionType != TransactionType.Unknown)
			.WithMessage(req => $"Transaction type '{req.TransactionType}' is not valid for the asset type.");
	}

	private void ConfigureAmountRules()
	{
		this.RuleFor(x => x.Units)
			.GreaterThanOrEqualTo(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction units must be greater than or equal to zero.");

		this.RuleFor(x => x.Price)
			.GreaterThanOrEqualTo(0)
			.When(x => TransactionValidationExtensions.IsUnitsRequired(x.TransactionType))
			.WithMessage("Transaction price must be greater than or equal to zero.");

		this.RuleFor(x => x.Amount)
			.GreaterThanOrEqualTo(0)
			.When(x => !TransactionValidationExtensions.IsUnitsRequired(x.TransactionType) && x.TransactionType != TransactionType.Unknown)
			.WithMessage("Transaction amount must be greater than or equal to zero.");
	}
}
