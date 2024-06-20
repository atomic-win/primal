using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddCashTransactionCommandHandler : IRequestHandler<AddCashTransactionCommand, ErrorOr<Transaction>>
{
	private readonly IAssetRepository assetRepository;
	private readonly IInstrumentRepository instrumentRepository;
	private readonly ITransactionRepository transactionRepository;

	public AddCashTransactionCommandHandler(
		IAssetRepository assetRepository,
		IInstrumentRepository instrumentRepository,
		ITransactionRepository transactionRepository)
	{
		this.assetRepository = assetRepository;
		this.instrumentRepository = instrumentRepository;
		this.transactionRepository = transactionRepository;
	}

	public async Task<ErrorOr<Transaction>> Handle(AddCashTransactionCommand request, CancellationToken cancellationToken)
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

		if (instrumentType == InstrumentType.MutualFunds)
		{
			return Error.Validation(description: "Mutual funds do not support cash transactions");
		}

		if (instrumentType == InstrumentType.Stocks
			&& request.Type != TransactionType.Dividend)
		{
			return Error.Validation(description: "Only dividends are supported for stock cash transactions");
		}

		if (instrumentType == InstrumentType.CashDeposits
			&& request.Type != TransactionType.Deposit
			&& request.Type != TransactionType.Withdrawal
			&& request.Type != TransactionType.Interest
			&& request.Type != TransactionType.SelfInterest)
		{
			return Error.Validation(description: "Only deposits, withdrawals, interest are supported for cash deposit accounts");
		}

		return await this.transactionRepository.AddCashTransactionAsync(
			request.UserId,
			request.Date,
			request.Name,
			request.Type,
			request.AssetId,
			request.Amount,
			request.Currency,
			cancellationToken);
	}
}
