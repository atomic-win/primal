using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetMutualFundBySchemeCodeQueryHandler : IRequestHandler<GetMutualFundBySchemeCodeQuery, ErrorOr<MutualFundResult>>
{
	private readonly IMutualFundRepository mutualFundRepository;
	private readonly IMutualFundApiClient mutualFundApiClient;

	public GetMutualFundBySchemeCodeQueryHandler(IMutualFundRepository mutualFundRepository, IMutualFundApiClient mutualFundApiClient)
	{
		this.mutualFundRepository = mutualFundRepository;
		this.mutualFundApiClient = mutualFundApiClient;
	}

	public async Task<ErrorOr<MutualFundResult>> Handle(GetMutualFundBySchemeCodeQuery request, CancellationToken cancellationToken)
	{
		var errorOrMutualFund = await this.mutualFundRepository.GetBySchemeCodeAsync(request.SchemeCode, cancellationToken);

		if (!errorOrMutualFund.IsError)
		{
			return this.MapToMutualFundResult(errorOrMutualFund);
		}

		if (errorOrMutualFund.FirstError is not { Type: ErrorType.NotFound })
		{
			return this.MapToMutualFundResult(errorOrMutualFund);
		}

		errorOrMutualFund = await this.mutualFundApiClient.GetBySchemeCodeAsync(request.SchemeCode, cancellationToken);

		if (errorOrMutualFund.IsError)
		{
			return this.MapToMutualFundResult(errorOrMutualFund);
		}

		var mutualFund = errorOrMutualFund.Value;

		errorOrMutualFund = await this.mutualFundRepository.AddAsync(
			mutualFund.SchemeName,
			mutualFund.FundHouse,
			mutualFund.SchemeType,
			mutualFund.SchemeCategory,
			mutualFund.SchemeCode,
			mutualFund.Currency,
			cancellationToken);

		return this.MapToMutualFundResult(errorOrMutualFund);
	}

	private ErrorOr<MutualFundResult> MapToMutualFundResult(ErrorOr<MutualFund> errorOrMutualFund)
	{
		return errorOrMutualFund.Match(
			mutualFund => new MutualFundResult(
			  mutualFund.Id,
			  mutualFund.SchemeName,
			  mutualFund.FundHouse,
			  mutualFund.SchemeType,
			  mutualFund.SchemeCategory,
			  mutualFund.SchemeCode,
			  mutualFund.Currency),
			errors => (ErrorOr<MutualFundResult>)errors);
	}
}
