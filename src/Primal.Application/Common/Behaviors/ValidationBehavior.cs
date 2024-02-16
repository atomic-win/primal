using ErrorOr;
using FluentValidation;
using MediatR;

namespace Primal.Application.Common.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
	where TResponse : IErrorOr
{
	private readonly IValidator<TRequest> validator;

	public ValidationBehavior(IValidator<TRequest> validator)
	{
		this.validator = validator;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var validationResult = await this.validator.ValidateAsync(request, cancellationToken);
		if (validationResult.IsValid)
		{
			return await next();
		}

		var errors = validationResult.Errors
			.ConvertAll(validationError => Error.Validation(validationError.PropertyName, validationError.ErrorMessage));

		return (dynamic)errors;
	}
}
