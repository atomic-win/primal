using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ErrorOr;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Authentication;

internal sealed class TokenIssuer : ITokenIssuer
{
	private readonly TokenIssuerSettings tokenIssuerSettings;

	private readonly TimeProvider timeProvider;

	private readonly SigningCredentials signingCredentials;

	private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler;

	internal TokenIssuer(IOptions<TokenIssuerSettings> tokenIssuerSettings, TimeProvider timeProvider)
	{
		this.tokenIssuerSettings = tokenIssuerSettings.Value;
		this.timeProvider = timeProvider;

		this.signingCredentials = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.tokenIssuerSettings.SecretKey)),
			SecurityAlgorithms.HmacSha256);

		this.jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
	}

	public async Task<ErrorOr<string>> IssueToken(User user, CancellationToken cancellationToken)
	{
		var claims = new Claim[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
		};

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = this.timeProvider.GetUtcNow().AddMinutes(this.tokenIssuerSettings.ExpirationInMinutes).UtcDateTime,
			Issuer = this.tokenIssuerSettings.Issuer,
			Audience = this.tokenIssuerSettings.Audience,
			SigningCredentials = this.signingCredentials,
		};

		return await Task.FromResult(this.jwtSecurityTokenHandler.CreateEncodedJwt(tokenDescriptor));
	}
}
