using Hangfire;
using Primal.Api.Middlewares;
using Primal.Application;
using Primal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Services
		.AddApplication()
		.AddInfrastructure(builder.Configuration)
		.AddPresentation();
}

var app = builder.Build();
{
	app.Services
		.GetService<IRecurringJobManagerV2>();

	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseUserMiddleware();
	app.UseAuthorization();
	app.UseHangfireDashboard();
	app.MapControllers();
	app.Run();
}
