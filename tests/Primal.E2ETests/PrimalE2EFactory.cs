using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Primal.Domain.Users;
using WireMock.Server;

#nullable enable

namespace Primal.E2ETests;

internal sealed class PrimalE2EFactory : WebApplicationFactory<Program>
{
	private const string TestSecretKey = "super-secret-test-key-that-is-long-enough-for-hmac-sha256";

	private readonly string dbPath = Path.Combine(Path.GetTempPath(), $"primal-e2e-{Guid.NewGuid()}.db");
	private readonly string cacheDbPath = Path.Combine(Path.GetTempPath(), $"primal-e2e-cache-{Guid.NewGuid()}.db");

	internal string DbPath => this.dbPath;

	internal WireMockServer MutualFundApi { get; } = WireMockServer.Start();

	internal WireMockServer StockApi { get; } = WireMockServer.Start();

	internal WireMockServer ExchangeRateApi { get; } = WireMockServer.Start();

	internal string CreateToken(UserId userId)
	{
		var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TestSecretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString("D")),
		};

		var token = new JwtSecurityToken(
			issuer: "TestIssuer",
			audience: "TestAudience",
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(60),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	internal HttpClient CreateAuthenticatedClient(UserId userId)
	{
		var client = this.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.CreateToken(userId));
		return client;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={this.dbPath}");
		builder.UseSetting("ConnectionStrings:CacheConnection", this.cacheDbPath);
		builder.UseSetting("TokenIssuerSettings:SecretKey", TestSecretKey);
		builder.UseSetting("TokenIssuerSettings:Issuer", "TestIssuer");
		builder.UseSetting("TokenIssuerSettings:Audience", "TestAudience");
		builder.UseSetting("TokenIssuerSettings:AccessTokenValidity", "60");
		builder.UseSetting("TokenIssuerSettings:RefreshTokenValidity", "120");
		builder.UseSetting("InvestmentSettings:FMPApiKey", "test-api-key");
		builder.UseSetting("InvestmentSettings:AlphaVantageApiKey", "test-alpha-key");
		builder.UseSetting("InvestmentSettings:MutualFundApiBaseUrl", this.MutualFundApi.Url!);
		builder.UseSetting("InvestmentSettings:StockApiBaseUrl", this.StockApi.Url!);
		builder.UseSetting("InvestmentSettings:ExchangeRateApiBaseUrl", this.ExchangeRateApi.Url!);
	}

	protected override void Dispose(bool disposing)
	{
		this.MutualFundApi.Stop();
		this.StockApi.Stop();
		this.ExchangeRateApi.Stop();

		if (File.Exists(this.dbPath))
		{
			File.Delete(this.dbPath);
		}

		if (File.Exists(this.cacheDbPath))
		{
			File.Delete(this.cacheDbPath);
		}
	}
}
