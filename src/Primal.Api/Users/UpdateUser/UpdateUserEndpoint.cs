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
		var userId = new UserId(req.UserId);
		var user = await this.userRepository.GetUserAsync(userId, ct);

		if (user.Id == UserId.Empty)
		{
			this.AddError("User not found", "USER_NOT_FOUND");
			this.ThrowIfAnyErrors(StatusCodes.Status404NotFound);
		}

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
}
