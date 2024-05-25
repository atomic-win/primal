using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddBuySellTransactionCommandHandler : IRequestHandler<AddBuySellTransactionCommand, ErrorOr<Transaction>>
{
	private readonly IAssetRepository assetRepository;
	private readonly IInstrumentRepository instrumentRepository;
	private readonly ITransactionRepository transactionRepository;

	public AddBuySellTransactionCommandHandler(IAssetRepository assetRepository, IInstrumentRepository instrumentRepository, ITransactionRepository transactionRepository)
	{
		this.assetRepository = assetRepository;
		this.instrumentRepository = instrumentRepository;
		this.transactionRepository = transactionRepository;
	}

	public async Task<ErrorOr<Transaction>> Handle(AddBuySellTransactionCommand request, CancellationToken cancellationToken)
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

		if (errorOrInstrument.Value.Type != InstrumentType.MutualFunds
			&& errorOrInstrument.Value.Type != InstrumentType.Stocks)
		{
			return Error.Validation("Only mutual funds and stocks are supported for buy/sell transactions");
		}

		return await this.transactionRepository.AddBuySellAsync(
			request.UserId,
			request.Date,
			request.Name,
			request.Type,
			request.AssetId,
			request.Units,
			cancellationToken);
	}
}
