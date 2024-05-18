using FluentValidation;

namespace Primal.Application.Investments;

internal sealed class GetAssetByIdQueryValidator : AbstractValidator<GetAssetByIdQuery>
{
	public GetAssetByIdQueryValidator()
	{
		this.RuleFor(x => x.UserId.Value).NotEmpty();
		this.RuleFor(x => x.AssetId.Value).NotEmpty();
	}
}
