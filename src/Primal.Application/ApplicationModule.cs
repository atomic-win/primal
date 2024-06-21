using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Primal.Application.Common.Behaviors;
using Primal.Application.Investments;

namespace Primal.Application;

public sealed class ApplicationModule : Autofac.Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<PortfolioCalculator>();

		this.RegisterMediatR(builder);
		this.RegisterValidators(builder);
	}

	private void RegisterMediatR(ContainerBuilder builder)
	{
		var configuration = MediatRConfigurationBuilder
			.Create(Assembly.GetExecutingAssembly())
			.WithAllOpenGenericHandlerTypesRegistered()
			.WithCustomPipelineBehavior(typeof(ValidationBehavior<,>))
			.Build();

		builder.RegisterMediatR(configuration);

		builder.RegisterGeneric(typeof(GetPortfolioQueryHandler<>))
			.As(typeof(IRequestHandler<,>));
	}

	private void RegisterValidators(ContainerBuilder builder)
	{
		builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
			.AsClosedTypesOf(typeof(IValidator<>))
			.AsImplementedInterfaces()
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(GetPortfolioQueryValidator<>))
			.As(typeof(IValidator<>));
	}
}
