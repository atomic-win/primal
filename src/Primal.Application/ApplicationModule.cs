using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Primal.Application.Common.Behaviors;

namespace Primal.Application;

public sealed class ApplicationModule : Autofac.Module
{
	protected override void Load(ContainerBuilder builder)
	{
		var configuration = MediatRConfigurationBuilder
			.Create(Assembly.GetExecutingAssembly())
			.WithAllOpenGenericHandlerTypesRegistered()
			.WithCustomPipelineBehavior(typeof(ValidationBehavior<,>))
			.Build();

		builder.RegisterMediatR(configuration);

		var openTypes = new[]
		{
			typeof(IValidator<>),
		};

		foreach (var openType in openTypes)
		{
			builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
				.AsClosedTypesOf(openType)
				.AsImplementedInterfaces()
				.InstancePerDependency();
		}
	}
}
