using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, ErrorOr<Asset>>
{
	private readonly IAssetRepository assetRepository;

	public GetAssetByIdQueryHandler(IAssetRepository assetRepository)
	{
		this.assetRepository = assetRepository;
	}

	public async Task<ErrorOr<Asset>> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
	{
		return await this.assetRepository.GetByIdAsync(request.UserId, request.AssetId, cancellationToken);
	}
}
