using ErrorOr;
using MediatR;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

public sealed record GetMutualFundByIdQuery(MutualFundId Id) : IRequest<ErrorOr<MutualFundResult>>;
