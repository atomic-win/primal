using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddCashDepositAssetCommand(UserId UserId, string Name) : IRequest<ErrorOr<Asset>>;
