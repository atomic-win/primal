using Autofac;
using InvestmentPortfolioTracker.Core.Investments;

namespace InvestmentPortfolioTracker.Core;

public sealed class CoreModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<TransactionAmountCalculator>()
				.As<ITransactionAmountCalculator>()
					.SingleInstance();
	}
}
