using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddMutualFundAssetCommandHandler : IRequestHandler<AddMutualFundAssetCommand, ErrorOr<Asset>>
{
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IInstrumentRepository instrumentRepository;
	private readonly IAssetRepository assetRepository;

	public AddMutualFundAssetCommandHandler(
		IMutualFundApiClient mutualFundApiClient,
		IInstrumentRepository instrumentRepository,
		IAssetRepository assetRepository)
	{
		this.mutualFundApiClient = mutualFundApiClient;
		this.instrumentRepository = instrumentRepository;
		this.assetRepository = assetRepository;
	}

	public async Task<ErrorOr<Asset>> Handle(AddMutualFundAssetCommand request, CancellationToken cancellationToken)
	{
		var errorOrMutualFund = await this.GetMutualFundAsync(request.SchemeCode, cancellationToken);

		if (errorOrMutualFund.IsError)
		{
			return errorOrMutualFund.Errors;
		}

		var mutualFund = errorOrMutualFund.Value;

		return await this.assetRepository.AddAsync(request.UserId, request.Name, mutualFund.Id, cancellationToken);
	}

	private async Task<ErrorOr<InvestmentInstrument>> GetMutualFundAsync(int schemeCode, CancellationToken cancellationToken)
	{
		var errorOrInvestmentInstrument = await this.instrumentRepository.GetMutualFundBySchemeCodeAsync(schemeCode, cancellationToken);

		if (!errorOrInvestmentInstrument.IsError)
		{
			return errorOrInvestmentInstrument;
		}

		if (errorOrInvestmentInstrument.IsError && errorOrInvestmentInstrument.FirstError is not { Type: ErrorType.NotFound })
		{
			return errorOrInvestmentInstrument.Errors;
		}

		var errorOrMutualFund = await this.mutualFundApiClient.GetBySchemeCodeAsync(schemeCode, cancellationToken);

		if (errorOrMutualFund.IsError)
		{
			return errorOrMutualFund.Errors;
		}

		var mutualFund = errorOrMutualFund.Value;

		return await this.instrumentRepository.AddMutualFundAsync(
			mutualFund.Name,
			mutualFund.FundHouse,
			mutualFund.SchemeType,
			mutualFund.SchemeCategory,
			mutualFund.SchemeCode,
			mutualFund.Currency,
			cancellationToken);
	}
}
