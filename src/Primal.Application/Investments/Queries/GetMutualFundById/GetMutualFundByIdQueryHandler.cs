using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetMutualFundByIdQueryHandler : IRequestHandler<GetMutualFundByIdQuery, ErrorOr<MutualFund>>
{
	private readonly IMutualFundRepository mutualFundRepository;

	public GetMutualFundByIdQueryHandler(IMutualFundRepository mutualFundRepository)
	{
		this.mutualFundRepository = mutualFundRepository;
	}

	public async Task<ErrorOr<MutualFund>> Handle(GetMutualFundByIdQuery request, CancellationToken cancellationToken)
	{
		return await this.mutualFundRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
