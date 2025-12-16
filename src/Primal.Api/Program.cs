using System.Text;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
				.RegisterModule<InfrastructureModule>();
		});

	builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Primal.Api")));

	builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false;
				options.SaveToken = true;

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = builder.Configuration["TokenIssuerSettings:Issuer"],
					ValidAudience = builder.Configuration["TokenIssuerSettings:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["TokenIssuerSettings:SecretKey"])),
					ClockSkew = TimeSpan.FromSeconds(5),
				};
			});

	builder.Services.AddAuthorization();

	builder.Services
		.AddInfrastructure(builder.Configuration);

	builder.Services.AddFastEndpoints();

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
	app.UseFastEndpoints(c =>
	{
		c.Endpoints.Configurator = epc =>
		{
			epc.PostProcessors(Order.After, typeof(EfSaveChangesPostProcessor<,>));
		};

		c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
	});

	app.Run();
}
