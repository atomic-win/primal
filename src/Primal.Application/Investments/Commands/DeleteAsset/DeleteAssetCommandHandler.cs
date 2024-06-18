using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, ErrorOr<Success>>
{
	private readonly IAssetRepository assetRepository;
	private readonly ITransactionRepository transactionRepository;

	public DeleteAssetCommandHandler(
		IAssetRepository assetRepository,
		ITransactionRepository transactionRepository)
	{
		this.assetRepository = assetRepository;
		this.transactionRepository = transactionRepository;
	}

	public async Task<ErrorOr<Success>> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
	{
		var errorOrTransactions = await this.transactionRepository.GetAllAsync(request.UserId, cancellationToken);

		if (errorOrTransactions.IsError)
		{
			return errorOrTransactions.Errors;
		}

		var assetTransactions = errorOrTransactions.Value.Where(x => x.AssetId == request.AssetId);

		if (assetTransactions.Any())
		{
			return Error.Conflict(description: "Existing transactions for the asset");
		}

		return await this.assetRepository.DeleteAsync(request.UserId, request.AssetId, cancellationToken);
	}
}
