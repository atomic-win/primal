using FluentValidation;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class GetTransactionsByAssetIdQueryValidator : AbstractValidator<GetTransactionsByAssetIdQuery>
{
	public GetTransactionsByAssetIdQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.AssetId.Value).NotEmpty();
		this.RuleFor(x => x.Currency).IsInEnum().NotEqual(Currency.Unknown);
	}
}
