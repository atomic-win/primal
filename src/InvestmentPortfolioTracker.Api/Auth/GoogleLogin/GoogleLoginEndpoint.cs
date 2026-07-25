using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using FastEndpoints;
using FastEndpoints.Security;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;

using InvestmentPortfolioTracker.Api.Errors;
using InvestmentPortfolioTracker.Core.Users;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Api.Auth;

[HttpPost("/api/auth/login/google")]
[AllowAnonymous]
internal sealed class GoogleLoginEndpoint : Endpoint<GoogleLoginRequest, TokenResponse>
{
	private readonly IIdTokenValidator idTokenValidator;
	private readonly IUserIdRepository userIdRepository;
	private readonly IUserRepository userRepository;

	public GoogleLoginEndpoint(
		IIdTokenValidator idTokenValidator,
		IUserIdRepository userIdRepository,
		IUserRepository userRepository)
	{
		this.idTokenValidator = idTokenValidator;
		this.userIdRepository = userIdRepository;
		this.userRepository = userRepository;
	}

	public override async Task HandleAsync(GoogleLoginRequest req, CancellationToken ct)
	{
		try
		{
			var identityProviderUser = await this.idTokenValidator.ValidateAsync(req.IdToken, ct);

			var userId = await this.userIdRepository.GetUserId(
				IdentityProvider.Google,
				identityProviderUser.Id,
				ct);

			if (userId == UserId.Empty)
			{
				var user = await this.userRepository.AddUserAsync(
					identityProviderUser.Email,
					firstName: identityProviderUser.FirstName,
					lastName: identityProviderUser.LastName,
					fullName: identityProviderUser.FullName,
					ct);

				userId = user.Id;

				await this.userIdRepository.AddUserId(
					IdentityProvider.Google,
					identityProviderUser.Id,
					userId,
					ct);
			}

			var tokenResponse = await this.CreateTokenWith<MyTokenService>(userId.ToString("D", CultureInfo.InvariantCulture), u =>
			{
				u.Claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString("D", CultureInfo.InvariantCulture)));
			});

			await this.Send.OkAsync(tokenResponse, cancellation: ct);
		}
		catch (InvalidJwtException ex) when (string.Equals(ex.Message, "JWT has expired.", StringComparison.OrdinalIgnoreCase))
		{
			this.AddError(ErrorMessages.Auth.IdTokenExpired, ErrorCodes.Auth.IdTokenExpired);
			await this.Send.ErrorsAsync(statusCode: 401, cancellation: ct);
		}
		catch (InvalidJwtException)
		{
			this.AddError(ErrorMessages.Auth.IdTokenInvalid, ErrorCodes.Auth.IdTokenInvalid);
			await this.Send.ErrorsAsync(statusCode: 401, cancellation: ct);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			this.AddError(ErrorMessages.Auth.UnexpectedError, ErrorCodes.Auth.UnexpectedError);
			await this.Send.ErrorsAsync(statusCode: 500, cancellation: ct);
		}
	}
}
