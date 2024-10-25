using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class GetTransactionsByAssetIdQueryHandler : IRequestHandler<GetTransactionsByAssetIdQuery, ErrorOr<IEnumerable<TransactionResult>>>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly InvestmentCalculator investmentCalculator;

	public GetTransactionsByAssetIdQueryHandler(
		ITransactionRepository transactionRepository,
		InvestmentCalculator investmentCalculator)
	{
		this.transactionRepository = transactionRepository;
		this.investmentCalculator = investmentCalculator;
	}

	public async Task<ErrorOr<IEnumerable<TransactionResult>>> Handle(GetTransactionsByAssetIdQuery request, CancellationToken cancellationToken)
	{
		var errorOrTransactions = await this.transactionRepository.GetByAssetIdAsync(
			request.UserId,
			request.AssetId,
			cancellationToken);

		if (errorOrTransactions.IsError)
		{
			return errorOrTransactions.Errors;
		}

		return await this.investmentCalculator.CalculateTransactionResultsAsync(
			request.UserId,
			request.Currency,
			errorOrTransactions.Value,
			cancellationToken);
	}
}
