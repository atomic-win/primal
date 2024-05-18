using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record GetAssetByIdQuery(UserId UserId, AssetId AssetId) : IRequest<ErrorOr<Asset>>;
