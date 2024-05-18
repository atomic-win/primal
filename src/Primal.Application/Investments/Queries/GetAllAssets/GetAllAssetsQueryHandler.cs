using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetAllAssetsQueryHandler : IRequestHandler<GetAllAssetsQuery, ErrorOr<IEnumerable<Asset>>>
{
	private readonly IAssetRepository assetRepository;

	public GetAllAssetsQueryHandler(IAssetRepository assetRepository)
	{
		this.assetRepository = assetRepository;
	}

	public async Task<ErrorOr<IEnumerable<Asset>>> Handle(GetAllAssetsQuery request, CancellationToken cancellationToken)
	{
		return await this.assetRepository.GetAllAsync(request.UserId, cancellationToken);
	}
}
