using Primal.Domain.Money;

namespace Primal.Application.Investments;

public sealed record Stock(
	string Symbol,
	string Name,
	Currency Currency);
