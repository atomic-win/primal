using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class GetValuationQueryValidator : AbstractValidator<GetValuationQuery>
{
	public GetValuationQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();

		this.RuleFor(x => x.Date).NotEqual(DateOnly.MinValue).NotEqual(DateOnly.MaxValue);

		this.RuleFor(x => x.AssetIds).NotEmpty();
		this.RuleForEach(x => x.AssetIds).ChildRules(assetId =>
		{
			assetId.RuleFor(x => x.Value).NotEmpty();
		});

		this.RuleFor(x => x.Currency).IsInEnum().NotEqual(Currency.Unknown);
	}
}
