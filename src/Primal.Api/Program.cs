using Autofac;
using Autofac.Extensions.DependencyInjection;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Primal.Api;
using Primal.Application;
using Primal.Infrastructure;
using Primal.Infrastructure.Persistence;

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

	builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Primal.Api")));

	builder.Services
		.AddInfrastructure(builder.Configuration);

	builder.Services.AddFastEndpoints();

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
	app.UseCors();
	app.UseAuthorization();
	app.UseFastEndpoints();
	app.Run();
}
