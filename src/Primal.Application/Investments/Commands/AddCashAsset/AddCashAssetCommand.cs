using ErrorOr;
using MediatR;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Application.Investments;

public sealed record AddCashAssetCommand(
	UserId UserId,
	string Name,
	InstrumentType Type,
	Currency Currency) : IRequest<ErrorOr<Asset>>;
