using FastEndpoints;
using FastEndpoints.Security;

namespace Primal.Api.Auth;

internal sealed class MyTokenService : RefreshTokenService<TokenRequest, TokenResponse>
{
	public MyTokenService(IConfiguration config)
	{
		this.Setup(o =>
		{
			o.TokenSigningKey = config["TokenIssuerSettings:SecretKey"] ?? throw new InvalidOperationException("TokenIssuerSettings:SecretKey configuration is missing");
			o.Issuer = config["TokenIssuerSettings:Issuer"] ?? throw new InvalidOperationException("TokenIssuerSettings:Issuer configuration is missing");
			o.Audience = config["TokenIssuerSettings:Audience"] ?? throw new InvalidOperationException("TokenIssuerSettings:Audience configuration is missing");
			o.AccessTokenValidity = TimeSpan.FromMinutes(config.GetValue<int>("TokenIssuerSettings:AccessTokenValidity"));
			o.RefreshTokenValidity = TimeSpan.FromHours(config.GetValue<int>("TokenIssuerSettings:RefreshTokenValidity"));
			o.Endpoint("/api/auth/refresh-token", ep =>
			{
				ep.Summary(s => s.Summary = "this is the refresh token endpoint");
			});
		});
	}

	public override async Task PersistTokenAsync(TokenResponse response)
	{
		await Task.CompletedTask;
	}

	public override Task RefreshRequestValidationAsync(TokenRequest req)
	{
		return Task.CompletedTask;
	}

	public override Task SetRenewalPrivilegesAsync(TokenRequest request, UserPrivileges privileges)
	{
		return Task.CompletedTask;
	}
}
