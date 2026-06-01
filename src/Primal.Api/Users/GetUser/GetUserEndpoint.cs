using FastEndpoints;
using Primal.Application.Users;
using Primal.Domain.Users;

namespace Primal.Api.Users;

[HttpGet("/api/users/me")]
internal sealed class GetUserEndpoint : Endpoint<GetUserRequest, UserResponse>
{
	private readonly IUserRepository userRepository;

	public GetUserEndpoint(IUserRepository userRepository)
	{
		this.userRepository = userRepository;
	}

	public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
	{
		var userId = new UserId(req.UserId);
		var user = await this.userRepository.GetUserAsync(userId, ct);

		if (user.Id == UserId.Empty)
		{
			this.ThrowError(new FluentValidation.Results.ValidationFailure("userId", "User not found") { ErrorCode = "USER_NOT_FOUND" }, StatusCodes.Status404NotFound);
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
