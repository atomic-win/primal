using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class AddCashAssetCommandHandler : IRequestHandler<AddCashAssetCommand, ErrorOr<Asset>>
{
	private readonly IInstrumentRepository instrumentRepository;
	private readonly IAssetRepository assetRepository;

	public AddCashAssetCommandHandler(IInstrumentRepository instrumentRepository, IAssetRepository assetRepository)
	{
		this.instrumentRepository = instrumentRepository;
		this.assetRepository = assetRepository;
	}

	public async Task<ErrorOr<Asset>> Handle(AddCashAssetCommand request, CancellationToken cancellationToken)
	{
		var errorOrCashInstrument = await this.GetCashInstrumentAsync(
			request.Type,
			request.Currency,
			cancellationToken);

		if (errorOrCashInstrument.IsError)
		{
			return errorOrCashInstrument.Errors;
		}

		var cashInstrument = errorOrCashInstrument.Value;

		return await this.assetRepository.AddAsync(
			request.UserId,
			request.Name,
			cashInstrument.Id,
			cancellationToken);
	}

	private async Task<ErrorOr<InvestmentInstrument>> GetCashInstrumentAsync(
		InstrumentType instrumentType,
		Currency currency,
		CancellationToken cancellationToken)
	{
		var errorOrInvestmentInstrument = await this.instrumentRepository.GetCashInstrumentAsync(
			instrumentType,
			currency,
			cancellationToken);

		if (!errorOrInvestmentInstrument.IsError)
		{
			return errorOrInvestmentInstrument;
		}

		if (errorOrInvestmentInstrument.IsError && errorOrInvestmentInstrument.FirstError is not { Type: ErrorType.NotFound })
		{
			return errorOrInvestmentInstrument.Errors;
		}

		return await this.instrumentRepository.AddCashInstrumentAsync(
			instrumentType,
			currency,
			cancellationToken);
	}
}
