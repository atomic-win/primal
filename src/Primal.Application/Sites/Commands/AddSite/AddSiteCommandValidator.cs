using FluentValidation;

namespace Primal.Application.Sites;

internal sealed class AddSiteCommandValidator : AbstractValidator<AddSiteCommand>
{
	public AddSiteCommandValidator()
	{
		this.RuleFor(x => x.UserId).NotEmpty();
		this.RuleFor(x => x.Host).NotEmpty();
		this.RuleFor(x => x.DailyLimitInMinutes).GreaterThan(0);
	}
}
