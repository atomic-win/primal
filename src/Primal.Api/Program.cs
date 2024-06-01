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
	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseUserMiddleware();
	app.UseAuthorization();
	app.MapControllers();
	app.Run();
}
