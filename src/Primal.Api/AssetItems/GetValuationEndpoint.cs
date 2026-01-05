using System.Collections.Immutable;
using FastEndpoints;
using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.AssetItems;

[HttpGet("/api/assetItems/valuation")]
internal sealed class GetValuationEndpoint : EndpointWithoutRequest<ValuationResponse>
{
	private readonly HybridCache cache;

	private readonly IAssetItemRepository assetItemRepository;
	private readonly ITransactionRepository transactionRepository;
	private readonly TransactionAmountCalculator transactionAmountCalculator;

	public GetValuationEndpoint(
		HybridCache cache,
		IAssetItemRepository assetItemRepository,
		ITransactionRepository transactionRepository,
		TransactionAmountCalculator transactionAmountCalculator)
	{
		this.cache = cache;
		this.assetItemRepository = assetItemRepository;
		this.transactionRepository = transactionRepository;
		this.transactionAmountCalculator = transactionAmountCalculator;
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var userId = this.GetUserId();
		var assetItemGuids = this.Query<IEnumerable<Guid>>("assetItemIds") ?? Array.Empty<Guid>();
		var valuationDate = this.Query<DateOnly>("date");
		var currency = this.Query<Currency>("currency");
		var assetItemIds = assetItemGuids
			.Distinct()
			.Order()
			.Select(id => new AssetItemId(id)).ToImmutableArray();

		await this.ValidateRequestParameters(userId, assetItemIds, valuationDate, currency, ct);
		this.ThrowIfAnyErrors();

		string cacheKey = $"users/{userId.Value}/assetItems/valuation?assetItemIds={string.Join(",", assetItemIds.Select(id => id.Value))}&date={valuationDate}&currency={currency}";
		var tags = assetItemIds
			.Select(id => $"users/{userId.Value}/assetItems/{id.Value}/valuation")
			.ToImmutableArray();

		var valuationResponse = await this.cache.GetOrCreateAsync(
			cacheKey,
			async _ => await this.CalculateValuationAsync(userId, assetItemIds, valuationDate, currency, ct),
			tags: tags,
			cancellationToken: ct);

		await this.Send.OkAsync(valuationResponse, ct);
	}

	private async Task ValidateRequestParameters(
		UserId userId,
		IReadOnlyCollection<AssetItemId> assetItemIds,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		if (valuationDate == default)
		{
			this.AddError("Date query parameter is required.");
		}

		if (valuationDate > DateOnly.FromDateTime(DateTime.UtcNow))
		{
			this.AddError("Valuation date cannot be in the future.");
		}

		if (currency == Currency.Unknown)
		{
			this.AddError("Currency query parameter is required.");
		}

		if (assetItemIds.Count == 0)
		{
			this.AddError("At least one assetItemGuid query parameter is required.");
		}

		foreach (var assetItemId in assetItemIds)
		{
			var assetItem = await this.assetItemRepository.GetByIdAsync(userId, assetItemId, ct);
			if (assetItem.Id == AssetItemId.Empty)
			{
				this.AddError($"Asset item with ID '{assetItemId.Value}' not found.");
			}
		}
	}

	private async Task<ValuationResponse> CalculateValuationAsync(
		UserId userId,
		IReadOnlyList<AssetItemId> assetItemIds,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		var valuationInputs = new List<ValuationInput>(capacity: assetItemIds.Count);
		foreach (var assetItemId in assetItemIds)
		{
			valuationInputs.Add(await this.GetValuationInputAsync(
				userId,
				assetItemId,
				valuationDate,
				currency,
				ct));
		}

		return new ValuationResponse(
			InvestedValue: valuationInputs.Sum(i => i.InvestedValue),
			CurrentValue: valuationInputs.Sum(i => i.CurrentValue),
			XirrPercent: 100 * this.CalculateXirr(valuationInputs.SelectMany(i => i.XirrInputs).ToImmutableArray()));
	}

	private async Task<ValuationInput> GetValuationInputAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		var transactions = (await this.transactionRepository.GetByAssetItemIdAsync(
			userId,
			assetItemId,
			valuationDate,
			ct)).ToImmutableArray();

		var investedValue = await this.CalculateInvestedValueAsync(
			userId,
			transactions,
			valuationDate,
			currency,
			ct);

		var currentValue = await this.CalculateCurrentValueAsync(
			userId,
			transactions,
			valuationDate,
			currency,
			ct);

		var xirrInputs = new List<XirrInput>(capacity: transactions.Length);
		foreach (var transaction in transactions)
		{
			xirrInputs.Add(
				await this.MapToXirrInputAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct));
		}

		return new ValuationInput
		{
			InvestedValue = investedValue,
			CurrentValue = currentValue,
			XirrInputs = xirrInputs,
		};
	}

	private async Task<XirrInput> MapToXirrInputAsync(
		UserId userId,
		Transaction transaction,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		return new XirrInput
		{
			YearDiff = (valuationDate.DayNumber - transaction.Date.DayNumber) / 365.25,
			TransactionAmount = await this.CalculateXIRRTransactionAmountAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct),
			BalanceAmount = await this.CalculateXIRRBalanceAmountAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct),
		};
	}

	private async Task<decimal> CalculateXIRRTransactionAmountAsync(
		UserId userId,
		Transaction transaction,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		return transaction.TransactionType switch
		{
			TransactionType.Buy or TransactionType.Deposit or TransactionType.InterestPenalty => await this.CalculateInitialAmountAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct),
			TransactionType.Sell or TransactionType.Withdrawal or TransactionType.Interest or TransactionType.Dividend => -await this.CalculateInitialAmountAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct),
			_ => 0m,
		};
	}

	private async Task<decimal> CalculateXIRRBalanceAmountAsync(
		UserId userId,
		Transaction transaction,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		return transaction.TransactionType switch
		{
			TransactionType.Buy or TransactionType.Deposit or TransactionType.SelfInterest => await this.CalculateCurrentAmountAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct),
			TransactionType.Sell or TransactionType.Withdrawal or TransactionType.InterestPenalty => -await this.CalculateCurrentAmountAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct),
			_ => 0m,
		};
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Refactoring would reduce readability")]
	private async Task<decimal> CalculateInvestedValueAsync(
			UserId userId,
			IEnumerable<Transaction> transactions,
			DateOnly valuationDate,
			Currency currency,
			CancellationToken ct)
	{
		decimal withdrawnCashUnits = transactions
			.Where(t => t.TransactionType == TransactionType.Withdrawal ||
						t.TransactionType == TransactionType.InterestPenalty)
			.Sum(t => t.Units);

		decimal withdrawnNonCashUnits = transactions
			.Where(t => t.TransactionType == TransactionType.Sell)
			.Sum(t => t.Units);

		decimal investedValue = 0;

		foreach (var transaction in transactions.OrderByDescending(t => t.Date))
		{
			if (transaction.TransactionType == TransactionType.Deposit)
			{
				var units = transaction.Units - Math.Min(withdrawnCashUnits, transaction.Units);
				withdrawnCashUnits -= transaction.Units - units;

				var newTransaction = new Transaction(
					transaction.Id,
					transaction.Date,
					transaction.Name,
					transaction.TransactionType,
					transaction.AssetItemId,
					units);

				investedValue += await this.CalculateInitialAmountAsync(
					userId,
					newTransaction,
					valuationDate,
					currency,
					ct);

				continue;
			}

			if (transaction.TransactionType == TransactionType.Buy)
			{
				var units = transaction.Units - Math.Min(withdrawnNonCashUnits, transaction.Units);
				withdrawnNonCashUnits -= transaction.Units - units;

				var newTransaction = new Transaction(
					transaction.Id,
					transaction.Date,
					transaction.Name,
					transaction.TransactionType,
					transaction.AssetItemId,
					units);

				investedValue += await this.CalculateInitialAmountAsync(
					userId,
					newTransaction,
					valuationDate,
					currency,
					ct);

				continue;
			}
		}

		return investedValue;
	}

	private async Task<decimal> CalculateCurrentValueAsync(
		UserId userId,
		IEnumerable<Transaction> transactions,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		var amounts = await Task.WhenAll(
			transactions.Select(async transaction =>
			{
				return transaction.TransactionType switch
				{
					TransactionType.Buy or TransactionType.Deposit or TransactionType.SelfInterest => await this.CalculateCurrentAmountAsync(
						userId,
						transaction,
						valuationDate,
						currency,
						ct),
					TransactionType.Sell => -await this.CalculateInitialAmountAsync(
						userId,
						transaction,
						valuationDate,
						currency,
						ct),
					TransactionType.Withdrawal or TransactionType.InterestPenalty => -await this.CalculateCurrentAmountAsync(
						userId,
						transaction,
						valuationDate,
						currency,
						ct),
					_ => 0m,
				};
			}).ToImmutableArray());

		return amounts.Sum();
	}

	private async Task<decimal> CalculateInitialAmountAsync(
		UserId userId,
		Transaction transaction,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		if (transaction.TransactionType == TransactionType.Deposit ||
			transaction.TransactionType == TransactionType.Withdrawal)
		{
			return await this.CalculateCurrentAmountAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct);
		}

		return await this.transactionAmountCalculator.CalculateAmountAsync(
			userId,
			transaction,
			transaction.Date,
			currency,
			ct);
	}

	private async Task<decimal> CalculateCurrentAmountAsync(
		UserId userId,
		Transaction transaction,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		return await this.transactionAmountCalculator.CalculateAmountAsync(
			userId,
			transaction,
			valuationDate,
			currency,
			ct);
	}

	private decimal CalculateXirr(
		IReadOnlyCollection<XirrInput> xirrInputs)
	{
		if (xirrInputs.Count == 0)
		{
			return 0;
		}

		decimal balanceAmount = xirrInputs.Sum(i => i.BalanceAmount);

		var allLessThanYear = xirrInputs.All(i => i.YearDiff < 1);

		var inValues = xirrInputs
			.Select(i => new { i.YearDiff, Value = i.TransactionAmount })
			.ToImmutableArray();

		if (allLessThanYear && balanceAmount != 0)
		{
			inValues = inValues
				.Select(i => new { YearDiff = 1.0, i.Value })
				.ToImmutableArray();
		}

		decimal xirrLowerBound = -1;
		decimal xirrUpperBound = 100;

		while (xirrUpperBound - xirrLowerBound > 0.0000001m)
		{
			decimal xirr = (xirrLowerBound + xirrUpperBound) / 2;
			decimal npv = inValues
				.Sum(i => i.Value * (decimal)Math.Pow((double)(1 + xirr), (double)i.YearDiff));

			if (npv > balanceAmount)
			{
				xirrUpperBound = xirr;
			}
			else
			{
				xirrLowerBound = xirr;
			}
		}

		return xirrUpperBound - xirrLowerBound <= 0.0000001m
			? xirrUpperBound
			: 0;
	}

	private sealed class ValuationInput
	{
		public required decimal InvestedValue { get; init; }

		public required decimal CurrentValue { get; init; }

		public required IReadOnlyCollection<XirrInput> XirrInputs { get; init; }
	}

	private sealed class XirrInput
	{
		public required double YearDiff { get; init; }

		public required decimal TransactionAmount { get; init; }

		public required decimal BalanceAmount { get; init; }
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "used only in this file")]
internal sealed record ValuationResponse(
	decimal InvestedValue,
	decimal CurrentValue,
	decimal XirrPercent);
