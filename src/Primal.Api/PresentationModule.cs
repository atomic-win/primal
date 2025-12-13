using System.Reflection;
using Autofac;
using Mapster;
using MapsterMapper;

namespace Primal.Api;

internal sealed class PresentationModule : Autofac.Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<HttpContextAccessor>()
			.As<IHttpContextAccessor>()
			.SingleInstance();

		this.RegisterMapster(builder);
	}

	private void RegisterMapster(ContainerBuilder builder)
	{
		var config = TypeAdapterConfig.GlobalSettings;
		config.Scan(Assembly.GetExecutingAssembly());

		builder.RegisterInstance(config);

		builder.RegisterType<ServiceMapper>()
			.As<IMapper>()
			.SingleInstance();
	}
}
