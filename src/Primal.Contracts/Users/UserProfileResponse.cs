using System.Net.Mail;

namespace Primal.Contracts.Users;

public sealed record UserProfileResponse(
	Guid Id,
	string Email,
	string FirstName,
	string LastName,
	string FullName,
	Uri ProfilePictureUrl);
