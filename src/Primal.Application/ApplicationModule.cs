using Autofac;
using Primal.Application.Investments;

namespace Primal.Application;

public sealed class ApplicationModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<TransactionAmountCalculator>()
			.InstancePerLifetimeScope();
	}
}
