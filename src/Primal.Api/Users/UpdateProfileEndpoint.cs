using FastEndpoints;
using Primal.Application.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

[HttpPatch("/api/users/profile")]
public sealed class UpdateProfileEndpoint : Endpoint<UpdateProfileRequest>
{
	private readonly IUserRepository userRepository;

	public UpdateProfileEndpoint(IUserRepository userRepository)
	{
		this.userRepository = userRepository;
	}

	public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
	{
		var userId = this.GetUserId();
		var user = await this.userRepository.GetUserAsync(userId, ct);

		if (user.Id == UserId.Empty)
		{
			this.ThrowError("User not found", 404);
			return;
		}

		if (user.PreferredCurrency == req.PreferredCurrency &&
			user.PreferredLocale == req.PreferredLocale)
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
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
public sealed record UpdateProfileRequest(
	Currency PreferredCurrency,
	Locale PreferredLocale);
