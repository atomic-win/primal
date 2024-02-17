using ErrorOr;

namespace Primal.Domain.Common.Errors;

public static partial class Errors
{
	public static class IdToken
	{
		public static Error Expired => Error.Unauthorized("IdTokenExpired", "The id token has expired.");

		public static Error Invalid => Error.Unauthorized("IdTokenInvalid", "The id token is invalid.");
	}
}
