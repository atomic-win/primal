using ErrorOr;
using MediatR;

namespace Primal.Application.Investments;

public sealed record GetMutualFundBySchemeCodeQuery(int SchemeCode) : IRequest<ErrorOr<MutualFundResult>>;
