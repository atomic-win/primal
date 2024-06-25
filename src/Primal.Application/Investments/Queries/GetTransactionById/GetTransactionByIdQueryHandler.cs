using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, ErrorOr<TransactionResult>>
{
	private readonly ITransactionRepository transactionRepository;
	private readonly InvestmentCalculator investmentCalculator;

	public GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository, InvestmentCalculator investmentCalculator)
	{
		this.transactionRepository = transactionRepository;
		this.investmentCalculator = investmentCalculator;
	}

	public async Task<ErrorOr<TransactionResult>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
	{
		var errorOrTransaction = await this.transactionRepository.GetByIdAsync(request.UserId, request.TransactionId, cancellationToken);

		if (errorOrTransaction.IsError)
		{
			return errorOrTransaction.Errors;
		}

		var errorOrTransactionResults = await this.investmentCalculator.CalculateTransactionResultsAsync(
			request.UserId,
			request.Currency,
			new[] { errorOrTransaction.Value },
			cancellationToken);

		if (errorOrTransactionResults.IsError)
		{
			return errorOrTransactionResults.Errors;
		}

		return errorOrTransactionResults.Value.Single();
	}
}
