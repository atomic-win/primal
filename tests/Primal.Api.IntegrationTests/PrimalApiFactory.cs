using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Primal.Application.Investments;
using Primal.Application.Users;
using Primal.Domain.Users;

#nullable enable

namespace Primal.Api.IntegrationTests;

internal sealed class PrimalApiFactory : WebApplicationFactory<Program>
{
	private const string TestSecretKey = "super-secret-test-key-that-is-long-enough-for-hmac-sha256";

	private readonly string cacheDbPath = Path.Combine(Path.GetTempPath(), $"primal-test-cache-{Guid.NewGuid()}.db");

	internal IUserRepository UserRepository { get; } = Substitute.For<IUserRepository>();

	internal IUserIdRepository UserIdRepository { get; } = Substitute.For<IUserIdRepository>();

	internal IAssetRepository AssetRepository { get; } = Substitute.For<IAssetRepository>();

	internal IAssetItemRepository AssetItemRepository { get; } = Substitute.For<IAssetItemRepository>();

	internal ITransactionRepository TransactionRepository { get; } = Substitute.For<ITransactionRepository>();

	internal ITransactionAmountCalculator TransactionAmountCalculator { get; } = Substitute.For<ITransactionAmountCalculator>();

	internal IExchangeRateApiClient ExchangeRateApiClient { get; } = Substitute.For<IExchangeRateApiClient>();

	internal string CreateToken(UserId userId)
	{
		var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TestSecretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString("D")),
			new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString("D")),
		};

		var token = new JwtSecurityToken(
			issuer: "TestIssuer",
			audience: "TestAudience",
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(60),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((context, config) =>
		{
			config.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
			{
				["TokenIssuerSettings:SecretKey"] = TestSecretKey,
				["TokenIssuerSettings:Issuer"] = "TestIssuer",
				["TokenIssuerSettings:Audience"] = "TestAudience",
				["TokenIssuerSettings:AccessTokenValidity"] = "60",
				["TokenIssuerSettings:RefreshTokenValidity"] = "120",
				["ConnectionStrings:DefaultConnection"] = $"Data Source={Path.Combine(Path.GetTempPath(), $"primal-test-{Guid.NewGuid()}.db")}",
				["ConnectionStrings:CacheConnection"] = this.cacheDbPath,
			});
		});
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
		{
			containerBuilder.RegisterInstance(this.UserRepository).As<IUserRepository>();
			containerBuilder.RegisterInstance(this.UserIdRepository).As<IUserIdRepository>();
			containerBuilder.RegisterInstance(this.AssetRepository).As<IAssetRepository>();
			containerBuilder.RegisterInstance(this.AssetItemRepository).As<IAssetItemRepository>();
			containerBuilder.RegisterInstance(this.TransactionRepository).As<ITransactionRepository>();
			containerBuilder.RegisterInstance(this.TransactionAmountCalculator).As<ITransactionAmountCalculator>();
			containerBuilder.RegisterInstance(this.ExchangeRateApiClient).As<IExchangeRateApiClient>();
		});

		return base.CreateHost(builder);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (File.Exists(this.cacheDbPath))
		{
			File.Delete(this.cacheDbPath);
		}
	}
}
