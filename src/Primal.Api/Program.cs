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
		.Enqueue<IMediator>(mediator => mediator.Send(new UpdateInstrumentValuesCommand(), app.Lifetime.ApplicationStopping));

	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseUserMiddleware();
	app.UseAuthorization();
	app.UseHangfireDashboard();
	app.MapControllers();
	app.Run();
}
