using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, ErrorOr<TransactionResult>>
{
	private readonly IAssetRepository assetRepository;
	private readonly IInstrumentRepository instrumentRepository;
	private readonly ITransactionRepository transactionRepository;

	public AddTransactionCommandHandler(
		IAssetRepository assetRepository,
		IInstrumentRepository instrumentRepository,
		ITransactionRepository transactionRepository)
	{
		this.assetRepository = assetRepository;
		this.instrumentRepository = instrumentRepository;
		this.transactionRepository = transactionRepository;
	}

	public async Task<ErrorOr<TransactionResult>> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
	{
		var errorOrTransaction = await this.CreateTransactionAsync(request, cancellationToken);

		if (errorOrTransaction.IsError)
		{
			return errorOrTransaction.Errors;
		}

		var transaction = errorOrTransaction.Value;

		return new TransactionResult(
			transaction.Id,
			transaction.Date,
			transaction.Name,
			transaction.Type,
			transaction.Units,
			0M);
	}

	private async Task<ErrorOr<Transaction>> CreateTransactionAsync(AddTransactionCommand request, CancellationToken cancellationToken)
	{
		var errorOrAsset = await this.assetRepository.GetByIdAsync(request.UserId, request.AssetId, cancellationToken);

		if (errorOrAsset.IsError)
		{
			return errorOrAsset.Errors;
		}

		var errorOrInstrument = await this.instrumentRepository.GetByIdAsync(errorOrAsset.Value.InstrumentId, cancellationToken);

		if (errorOrInstrument.IsError)
		{
			return errorOrInstrument.Errors;
		}

		var instrumentType = errorOrInstrument.Value.Type;

		if (instrumentType == InstrumentType.MutualFunds
			&& request.Type != TransactionType.Buy
			&& request.Type != TransactionType.Sell)
		{
			return Error.Validation(description: "Only buy and sell transactions are supported for mutual funds");
		}

		if (instrumentType == InstrumentType.Stocks
			&& request.Type != TransactionType.Buy
			&& request.Type != TransactionType.Sell
			&& request.Type != TransactionType.Dividend)
		{
			return Error.Validation(description: "Only buy, sell and dividend transactions are supported for stocks");
		}

		if ((instrumentType == InstrumentType.EmergencyFunds
				|| instrumentType == InstrumentType.CashAccounts
				|| instrumentType == InstrumentType.FixedDeposits
				|| instrumentType == InstrumentType.EPF
				|| instrumentType == InstrumentType.PPF)
			&& request.Type != TransactionType.Deposit
			&& request.Type != TransactionType.Withdrawal
			&& request.Type != TransactionType.Interest
			&& request.Type != TransactionType.SelfInterest
			&& request.Type != TransactionType.InterestPenalty)
		{
			return Error.Validation(description: $"Only deposit, withdrawal, interest, self-interest and interest penalty transactions are supported for {instrumentType}");
		}

		return await this.transactionRepository.AddAsync(
			request.UserId,
			request.AssetId,
			request.Date,
			request.Name,
			request.Type,
			request.Units,
			cancellationToken);
	}
}
