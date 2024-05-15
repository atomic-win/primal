using ErrorOr;
using MediatR;

namespace Primal.Application.Investments;

public sealed record GetStockBySymbolQuery(string Symbol) : IRequest<ErrorOr<StockResult>>;
