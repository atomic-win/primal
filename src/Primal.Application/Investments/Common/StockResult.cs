using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

public sealed record StockResult(StockId StockId, string Symbol, string Name, string Region, Currency Currency);
