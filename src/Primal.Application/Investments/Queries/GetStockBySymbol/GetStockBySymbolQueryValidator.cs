using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetStockBySymbolQueryValidator : AbstractValidator<GetStockBySymbolQuery>
{
	public GetStockBySymbolQueryValidator()
	{
		this.RuleFor(x => x.Symbol).NotEmpty();
	}
}
