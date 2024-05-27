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
		.GetService<IBackgroundJobClientV2>()
		.Schedule<IMediator>(mediator => mediator.Send(new UpdateInstrumentValuesCommand(), app.Lifetime.ApplicationStopping), delay: TimeSpan.FromSeconds(30));

	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseUserMiddleware();
	app.UseAuthorization();
	app.UseHangfireDashboard();
	app.MapControllers();
	app.Run();
}
