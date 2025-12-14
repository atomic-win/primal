using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Primal.Application.Users;
using Primal.Domain.Users;

namespace Primal.Api.Auth;

[HttpPost("/api/auth/login/google")]
[AllowAnonymous]
public sealed class GoogleLoginEndpoint : Endpoint<LoginRequest, TokenResponse>
{
	private readonly IUserIdRepository userIdRepository;
	private readonly IUserRepository userRepository;

	public GoogleLoginEndpoint(
		IUserIdRepository userIdRepository,
		IUserRepository userRepository)
	{
		this.userIdRepository = userIdRepository;
		this.userRepository = userRepository;
	}

	public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
	{
		try
		{
			GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken);

			var userId = await this.userIdRepository.GetUserId(
				IdentityProvider.Google,
				new IdentityProviderUserId(payload.Subject),
				ct);

			if (userId == UserId.Empty)
			{
				userId = await this.userIdRepository.AddUserId(
					IdentityProvider.Google,
					new IdentityProviderUserId(payload.Subject),
					ct);

				await this.userRepository.AddUserAsync(
					userId,
					payload.Email,
					firstName: payload.GivenName,
					lastName: payload.FamilyName,
					fullName: payload.Name,
					ct);
			}

			var tokenResponse = await this.CreateTokenWith<MyTokenService>(userId.ToString(), u =>
			{
				u.Claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));
			});

			await this.Send.OkAsync(tokenResponse, cancellation: ct);
		}
		catch (InvalidJwtException ex) when (string.Equals(ex.Message, "JWT has expired.", StringComparison.OrdinalIgnoreCase))
		{
			this.AddError("IdToken", "ID token has expired.");
			await this.Send.ErrorsAsync(statusCode: 401, cancellation: ct);
		}
		catch (InvalidJwtException)
		{
			this.AddError("IdToken", "ID token is invalid.");
			await this.Send.ErrorsAsync(statusCode: 401, cancellation: ct);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			this.AddError("IdToken", "An unexpected error occurred");
			await this.Send.ErrorsAsync(statusCode: 500, cancellation: ct);
		}
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
public sealed record LoginRequest(string IdToken);
