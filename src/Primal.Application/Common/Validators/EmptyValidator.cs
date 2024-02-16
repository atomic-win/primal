using FluentValidation;

namespace Primal.Application.Common.Validators;

internal sealed class EmptyValidator<T> : AbstractValidator<T>
{
	public EmptyValidator()
	{
	}
}
