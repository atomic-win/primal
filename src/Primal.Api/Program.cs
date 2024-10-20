using Autofac;
using Autofac.Extensions.DependencyInjection;
using Primal.Api;
using Primal.Api.Middlewares;
using Primal.Application;
using Primal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Host
		.UseServiceProviderFactory(new AutofacServiceProviderFactory())
		.ConfigureContainer<ContainerBuilder>(builder =>
		{
			builder
				.RegisterModule<ApplicationModule>()
				.RegisterModule<InfrastructureModule>()
				.RegisterModule<PresentationModule>();
		});

	builder.Services
		.AddInfrastructure(builder.Configuration);

	builder.Services.AddControllers().AddNewtonsoftJson();

	builder.Services.AddCors(options =>
	{
		options.AddDefaultPolicy(
			policy =>
			{
				policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
					.AllowAnyHeader()
					.AllowAnyMethod();
			});
	});
}

var app = builder.Build();
{
	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseUserMiddleware();
	app.UseCors();
	app.UseAuthorization();
	app.MapControllers();
	app.Run();
}
