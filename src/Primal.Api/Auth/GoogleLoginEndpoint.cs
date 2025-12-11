using FastEndpoints;
using FastEndpoints.Security;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Primal.Api.Auth;

[HttpPost("/api/auth/login/google")]
[AllowAnonymous]
public sealed class GoogleLoginEndpoint : Endpoint<LoginRequest, TokenResponse>
{
	public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
	{
		try
		{
			GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken);

			var tokenResponse = await this.CreateTokenWith<MyTokenService>(payload.Subject, u =>
			{
				u.Roles.AddRange(new[] { "Admin", "Manager" });
				u.Permissions.Add("Update_Something");
				u.Claims.Add(new("UserId", "user-id-001"));
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
