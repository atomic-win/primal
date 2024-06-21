using System.Reflection;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Primal.Application.Common.Behaviors;
using Primal.Application.Common.Validators;
using Primal.Application.Investments;
using Primal.Domain.Investments;

namespace Primal.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

		services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

		services.AddScoped(typeof(IValidator<>), typeof(EmptyValidator<>));

		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

		services.AddSingleton<PortfolioCalculator>();

		return services;
	}
}
