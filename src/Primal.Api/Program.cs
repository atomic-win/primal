using Primal.Application;
using Primal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Services
		.AddApplication()
		.AddInfrastructure()
		.AddPresentation();
}

var app = builder.Build();
{
	app.UseHttpsRedirection();
	app.MapControllers();
	app.Run();
}
