using FastEndpoints;
using Primal.Application.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

[HttpPatch("/api/users/me")]
internal sealed class UpdateUserEndpoint : Endpoint<UpdateUserRequest>
{
	private readonly IUserRepository userRepository;

	public UpdateUserEndpoint(IUserRepository userRepository)
	{
		this.userRepository = userRepository;
	}

	public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
	{
		var userId = this.GetUserId();
		var user = await this.userRepository.GetUserAsync(userId, ct);

		if (user.Id == UserId.Empty)
		{
			this.ThrowError("User not found", 404);
			return;
		}

		this.ValidateRequest(req);

		if ((req.PreferredCurrency == Currency.Unknown || req.PreferredCurrency == user.PreferredCurrency)
			&& (req.PreferredLocale == Locale.Unknown || req.PreferredLocale == user.PreferredLocale))
		{
			await this.Send.NoContentAsync(ct);
			return;
		}

		await this.userRepository.UpdateUserProfileAsync(
			userId,
			req.PreferredCurrency == Currency.Unknown ? user.PreferredCurrency : req.PreferredCurrency,
			req.PreferredLocale == Locale.Unknown ? user.PreferredLocale : req.PreferredLocale,
			ct);

		await this.Send.NoContentAsync(ct);
	}

	private void ValidateRequest(UpdateUserRequest req)
	{
		if (req.PreferredCurrency == Currency.Unknown &&
			req.PreferredLocale == Locale.Unknown)
		{
			this.ThrowError("At least one field of preferred currency or preferred locale must be provided", 400);
		}
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "used only in this file")]
internal sealed record UpdateUserRequest(
	Currency PreferredCurrency,
	Locale PreferredLocale);
