using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddStockAssetCommand(UserId UserId, string Name, string Symbol) : IRequest<ErrorOr<Asset>>;
