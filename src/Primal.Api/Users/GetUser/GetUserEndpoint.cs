using FastEndpoints;
using Primal.Application.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Users;

[HttpGet("/api/users/me")]
internal sealed class GetUserEndpoint : EndpointWithoutRequest<UserResponse>
{
	private readonly IUserRepository userRepository;

	public GetUserEndpoint(IUserRepository userRepository)
	{
		this.userRepository = userRepository;
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var userId = this.GetUserId();
		var user = await this.userRepository.GetUserAsync(userId, ct);

		if (user.Id == UserId.Empty)
		{
			this.ThrowError("User not found", 404);
			return;
		}

		await this.Send.OkAsync(
			new UserResponse(
			user.Id.Value,
			user.Email,
			user.FirstName,
			user.LastName,
			user.FullName,
			user.PreferredCurrency,
			user.PreferredLocale),
			ct);
	}
}
