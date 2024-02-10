namespace Primal.Infrastructure.Authentication;

internal sealed class TokenIssuerSettings
{
	internal const string SectionName = nameof(TokenIssuerSettings);

	public string SecretKey { get; init; }

	public int ExpirationInMinutes { get; init; }

	public string Issuer { get; init; }

	public string Audience { get; init; }
}
