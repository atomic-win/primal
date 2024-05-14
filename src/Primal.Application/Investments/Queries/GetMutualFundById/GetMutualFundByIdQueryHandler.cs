using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class GetMutualFundByIdQueryHandler : IRequestHandler<GetMutualFundByIdQuery, ErrorOr<MutualFundResult>>
{
	private readonly IMutualFundRepository mutualFundRepository;

	public GetMutualFundByIdQueryHandler(IMutualFundRepository mutualFundRepository)
	{
		this.mutualFundRepository = mutualFundRepository;
	}

	public async Task<ErrorOr<MutualFundResult>> Handle(GetMutualFundByIdQuery request, CancellationToken cancellationToken)
	{
		var errorOrMutualFund = await this.mutualFundRepository.GetByIdAsync(request.Id, cancellationToken);

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
