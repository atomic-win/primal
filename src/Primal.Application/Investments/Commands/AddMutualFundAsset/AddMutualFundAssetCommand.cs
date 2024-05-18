using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddMutualFundAssetCommand(UserId UserId, string Name, int SchemeCode) : IRequest<ErrorOr<Asset>>;
