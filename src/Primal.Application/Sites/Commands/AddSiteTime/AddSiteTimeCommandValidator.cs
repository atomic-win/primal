using FluentValidation;

namespace Primal.Application.Sites;

internal sealed class AddSiteTimeCommandValidator : AbstractValidator<AddSiteTimeCommand>
{
	public AddSiteTimeCommandValidator()
	{
		this.RuleFor(command => command.UserId).NotEmpty();
		this.RuleFor(command => command.SiteId).NotEmpty();
		this.RuleFor(command => command.Time).GreaterThan(DateTime.MinValue).LessThan(DateTime.UtcNow);
	}
}
