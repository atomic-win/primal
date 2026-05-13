using System.Text;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using NeoSmart.Caching.Sqlite;
using Primal.Application;
using Primal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Host
		.UseServiceProviderFactory(new AutofacServiceProviderFactory())
		.ConfigureContainer<ContainerBuilder>(containerBuilder =>
		{
			containerBuilder
				.RegisterModule<ApplicationModule>()
				.RegisterModule<InfrastructureModule>();
		});

	builder.Services.AddHybridCache(options =>
	{
		options.DefaultEntryOptions = new HybridCacheEntryOptions
		{
			Expiration = TimeSpan.FromDays(1),
			Flags = HybridCacheEntryFlags.DisableDistributedCache,
		};
	});

	builder.Services.AddSqliteCache(options =>
	{
		options.CachePath = builder.Configuration.GetConnectionString("CacheConnection");
	});

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
		c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
	});

	app.Run();
}
