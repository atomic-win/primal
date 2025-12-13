using FastEndpoints;
using Primal.Application.Users;
using Primal.Domain.Users;

namespace Primal.Api.Users;

[HttpGet("/api/users/profile")]
public sealed class GetProfileEndpoint : EndpointWithoutRequest<UserProfileResponse>
{
	private readonly IUserRepository userRepository;

	public GetProfileEndpoint(IUserRepository userRepository)
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
			new UserProfileResponse(
			user.Id.Value,
			user.Email.Address,
			user.FirstName,
			user.LastName,
			user.FullName),
			ct);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "used only in this file")]
public sealed record UserProfileResponse(
	Guid Id,
	string Email,
	string FirstName,
	string LastName,
	string FullName);
