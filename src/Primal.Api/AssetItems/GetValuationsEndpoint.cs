using System.Collections.Immutable;
using FastEndpoints;
using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.AssetItems;

[HttpGet("/api/assetItems/valuations")]
internal sealed class GetValuationsEndpoint : EndpointWithoutRequest<IEnumerable<ValuationResponse>>
{
	private readonly HybridCache cache;

	private readonly IAssetItemRepository assetItemRepository;
	private readonly ITransactionRepository transactionRepository;
	private readonly TransactionAmountCalculator transactionAmountCalculator;

	public GetValuationsEndpoint(
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
		var currency = this.Query<Currency>("currency");
		var assetItemIds = assetItemGuids
			.Distinct()
			.Order()
			.Select(id => new AssetItemId(id)).ToImmutableArray();

		await this.ValidateRequestParameters(userId, assetItemIds, currency, ct);
		this.ThrowIfAnyErrors();

		await this.Send.OkAsync(await this.CalculateValuationsAsync(userId, assetItemIds, currency, ct), ct);
	}

	private async Task ValidateRequestParameters(
		UserId userId,
		IReadOnlyCollection<AssetItemId> assetItemIds,
		Currency currency,
		CancellationToken ct)
	{
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

	private async Task<IEnumerable<ValuationResponse>> CalculateValuationsAsync(
		UserId userId,
		IReadOnlyList<AssetItemId> assetItemIds,
		Currency currency,
		CancellationToken ct)
	{
		var valuationInputs = new List<ValuationInput>();

		foreach (var valuationDate in this.GetValuationDates())
		{
			var valuationInput = await this.CalculateValuationInputAsync(
				userId,
				assetItemIds,
				valuationDate,
				currency,
				ct);

			if (valuationInput.XirrInputs.Count == 0)
			{
				break;
			}

			valuationInputs.Add(valuationInput);
		}

		return valuationInputs
			.AsParallel()
			.WithCancellation(ct)
			.Select(i => new ValuationResponse(
				Date: i.Date,
				InvestedValue: i.InvestedValue,
				CurrentValue: i.CurrentValue,
				XirrPercent: 100 * this.CalculateXirr(i.XirrInputs, ct)));
	}

	private async Task<ValuationInput> CalculateValuationInputAsync(
		UserId userId,
		IReadOnlyList<AssetItemId> assetItemIds,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		var valuationInputs = new List<ValuationInput>(capacity: assetItemIds.Count);
		foreach (var assetItemId in assetItemIds)
		{
			valuationInputs.Add(await this.cache.GetOrCreateAsync(
				key: $"users/{userId.Value}/assetItems/{assetItemId.Value}/valuationInput?valuationDate={valuationDate}&currency={currency}",
				async _ => await this.CalculateValuationInputAsync(
					userId,
					assetItemId,
					valuationDate,
					currency,
					ct),
				tags: new[] { $"users/{userId.Value}/assetItems/{assetItemId.Value}/valuations" },
				cancellationToken: ct));
		}

		return new ValuationInput
		{
			Date = valuationDate,
			InvestedValue = valuationInputs.Sum(i => i.InvestedValue),
			CurrentValue = valuationInputs.Sum(i => i.CurrentValue),
			XirrInputs = valuationInputs.SelectMany(i => i.XirrInputs).ToImmutableArray(),
		};
	}

	private async Task<ValuationInput> CalculateValuationInputAsync(
		UserId userId,
		AssetItemId assetItemId,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		var allTransactions = await this.transactionRepository.GetByAssetItemIdAsync(
			userId,
			assetItemId,
			ct);

		var transactionsWithinValuationDate = allTransactions
			.Where(t => t.Date <= valuationDate)
			.ToImmutableArray();

		var investedValue = await this.CalculateInvestedValueAsync(
			userId,
			transactionsWithinValuationDate,
			valuationDate,
			currency,
			ct);

		var currentValue = await this.CalculateCurrentValueAsync(
			userId,
			transactionsWithinValuationDate,
			valuationDate,
			currency,
			ct);

		var xirrInputs = transactionsWithinValuationDate
			.Select(async transaction => await this.MapToXirrInputAsync(
				userId,
				transaction,
				valuationDate,
				currency,
				ct))
			.Select(t => t.Result)
			.ToImmutableArray();

		return new ValuationInput
		{
			Date = valuationDate,
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

	private async Task<decimal> CalculateInvestedValueAsync(
		UserId userId,
		IEnumerable<Transaction> transactions,
		DateOnly valuationDate,
		Currency currency,
		CancellationToken ct)
	{
		// !!! NOTE: This method calculates the invested value for only a single asset item
		decimal withdrawnAmount = transactions
			.Where(t => t.TransactionType == TransactionType.Withdrawal ||
						t.TransactionType == TransactionType.InterestPenalty)
			.Sum(t => t.Amount);

		decimal withdrawnUnits = transactions
			.Where(t => t.TransactionType == TransactionType.Sell)
			.Sum(t => t.Units);

		decimal investedValue = 0;

		foreach (var transaction in transactions.OrderBy(t => t.Date))
		{
			if (transaction.TransactionType != TransactionType.Deposit &&
				transaction.TransactionType != TransactionType.Buy)
			{
				continue;
			}

			var currentTransactionWithDrawnAmount = Math.Min(withdrawnAmount, transaction.Amount);
			var currentTransactionWithDrawnUnits = Math.Min(withdrawnUnits, transaction.Units);

			withdrawnAmount -= currentTransactionWithDrawnAmount;
			withdrawnUnits -= currentTransactionWithDrawnUnits;

			if (transaction.Amount == currentTransactionWithDrawnAmount &&
				transaction.Units == currentTransactionWithDrawnUnits)
			{
				continue;
			}

			var newTransaction = new Transaction(
				transaction.Id,
				transaction.Date,
				transaction.Name,
				transaction.TransactionType,
				transaction.AssetItemId,
				transaction.Units - currentTransactionWithDrawnUnits,
				transaction.Price,
				transaction.Amount - currentTransactionWithDrawnAmount);

			investedValue += await this.CalculateInitialAmountAsync(
				userId,
				newTransaction,
				valuationDate,
				currency,
				ct);
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
		return await this.transactionAmountCalculator.CalculateAmountAsync(
			userId,
			transaction,
			transaction.TransactionType == TransactionType.Deposit || transaction.TransactionType == TransactionType.Withdrawal ? valuationDate : transaction.Date,
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
		IReadOnlyCollection<XirrInput> xirrInputs,
		CancellationToken ct)
	{
		if (xirrInputs.Count == 0)
		{
			return 0;
		}

		xirrInputs = xirrInputs
			.GroupBy(i => i.YearDiff)
			.AsParallel()
			.WithCancellation(ct)
			.Select(g => new XirrInput
			{
				YearDiff = g.Key,
				TransactionAmount = g.Sum(i => i.TransactionAmount),
				BalanceAmount = g.Sum(i => i.BalanceAmount),
			})
			.ToImmutableArray();

		decimal balanceAmount = xirrInputs.Sum(i => i.BalanceAmount);

		var allLessThanYear = xirrInputs.All(i => i.YearDiff < 1);

		var inValues = xirrInputs
			.AsParallel()
			.WithCancellation(ct)
			.Where(i => i.TransactionAmount != 0)
			.Select(i => new
			{
				YearDiff = allLessThanYear && balanceAmount != 0 ? 1.0 : i.YearDiff,
				Value = i.TransactionAmount,
			})
			.ToImmutableArray();

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

	private IEnumerable<DateOnly> GetValuationDates()
	{
		yield return DateOnly.FromDateTime(DateTime.UtcNow);

		var endOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1);

		while (true)
		{
			yield return endOfMonth;
			endOfMonth = new DateOnly(endOfMonth.Year, endOfMonth.Month, 1).AddDays(-1);
		}
	}

	private sealed class ValuationInput
	{
		public required DateOnly Date { get; init; }

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
	DateOnly Date,
	decimal InvestedValue,
	decimal CurrentValue,
	decimal XirrPercent);
