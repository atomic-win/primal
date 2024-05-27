using Hangfire;
using MediatR;
using Primal.Api.Middlewares;
using Primal.Application;
using Primal.Application.Investments;
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
		.GetService<IRecurringJobManagerV2>()
		.AddOrUpdate<IMediator>("UpdateInstrumentValues", mediator => mediator.Send(new UpdateInstrumentValuesCommand(), app.Lifetime.ApplicationStopping), Cron.Minutely);

	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseUserMiddleware();
	app.UseAuthorization();
	app.UseHangfireDashboard();
	app.MapControllers();
	app.Run();
}
