using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, ErrorOr<TransactionResult>>
{
	private readonly IAssetRepository assetRepository;
	private readonly ITransactionRepository transactionRepository;

	private readonly TransactionResultCalculator transactionResultCalculator;

	public GetTransactionByIdQueryHandler(
		IAssetRepository assetRepository,
		ITransactionRepository transactionRepository,
		TransactionResultCalculator transactionResultCalculator)
	{
		this.assetRepository = assetRepository;
		this.transactionRepository = transactionRepository;
		this.transactionResultCalculator = transactionResultCalculator;
	}

	public async Task<ErrorOr<TransactionResult>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
	{
		var errorOrAsset = await this.assetRepository.GetByIdAsync(
			request.UserId,
			request.AssetId,
			cancellationToken);

		if (errorOrAsset.IsError)
		{
			return errorOrAsset.Errors;
		}

		var errorOrTransaction = await this.transactionRepository.GetByIdAsync(
			request.UserId,
			request.AssetId,
			request.TransactionId,
			cancellationToken);

		if (errorOrTransaction.IsError)
		{
			return errorOrTransaction.Errors;
		}

		var errorOrTransactionResults = await this.transactionResultCalculator.CalculateAsync(
			errorOrAsset.Value,
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
